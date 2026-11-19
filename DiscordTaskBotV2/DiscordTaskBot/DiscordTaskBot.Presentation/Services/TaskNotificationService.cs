namespace DiscordTaskBot.Presentation;

using Discord;
using Discord.Net;
using Discord.WebSocket;
using DiscordTaskBot.Application;
using DiscordTaskBot.Domain;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

public enum DmOutcome
{
    Delivered,
    // DMs closed / user gone - retrying will not help.
    Undeliverable,
    // Network error, Discord outage... - retrying later could succeed.
    Transient
}

/// <param name="FailedUserIds">Users that did not get the DM.</param>
/// <param name="NothingDelivered">True when no DM went out and at least one failure was transient (worth retrying).</param>
public record DmResult(IReadOnlyList<ulong> FailedUserIds, bool NothingDelivered)
{
    public bool AllDelivered => FailedUserIds.Count == 0;

    public string FailedMentions => string.Join(", ", FailedUserIds.Select(id => $"<@{id}>"));
}

/// <summary>
/// Direct messages about tasks. Never throws. Each assignee gets at most:
/// one message when assigned, one "due soon" reminder, one "overdue" reminder,
/// and one message per burst of deadline changes (see <see cref="ScheduleDeadlineChanged"/>).
/// </summary>
public class TaskNotificationService(
    DiscordSocketClient client,
    ITaskRepository repository,
    IOptions<DiscordBotOptions> options,
    ILogger<TaskNotificationService> logger)
{
    public static readonly TimeSpan DeadlineChangeDelay = TimeSpan.FromMinutes(3);

    private readonly DiscordSocketClient _client = client;
    private readonly ITaskRepository _repository = repository;
    private readonly DiscordBotOptions _options = options.Value;
    private readonly ILogger<TaskNotificationService> _logger = logger;

    private readonly object _pendingLock = new();
    private readonly Dictionary<Guid, PendingDeadlineChange> _pendingDeadlineChanges = [];

    private sealed class PendingDeadlineChange(DateTime originalUtcDueDate)
    {
        public DateTime OriginalUtcDueDate { get; } = originalUtcDueDate;
        public CancellationTokenSource Cts { get; set; } = new();
    }

    /// <summary>"New task" message - to all assignees, or only to <paramref name="userIds"/> (e.g. newly added ones).</summary>
    public Task<DmResult> SendNewTaskAsync(TaskItem task, IEnumerable<ulong>? userIds = null)
        => SendToUsersAsync(userIds ?? task.AssigneeIDs,
            "🎯 **New Task Alert!**\n\n" +
            $"**{task.Description}**\n\n" +
            $"📅 Due: {DiscordUtility.GetDiscordTimestamp(task.TaskDuration.UtcDueDate, 'R')}\n" +
            LocationLine(task));

    public Task<DmResult> SendReminderAsync(TaskItem task, ReminderKind kind)
    {
        var header = kind == ReminderKind.Overdue
            ? "⚠️ **Task overdue!** The deadline has passed."
            : "🔔 **Reminder: the deadline is approaching!**";

        return SendToUsersAsync(task.PendingAssigneeIDs,
            $"{header}\n\n" +
            $"**{task.Description}**\n\n" +
            $"📅 Deadline: {DiscordUtility.GetDiscordTimestamp(task.TaskDuration.UtcDueDate, 'f')} " +
            $"({DiscordUtility.GetDiscordTimestamp(task.TaskDuration.UtcDueDate, 'R')})\n" +
            LocationLine(task));
    }

    /// <summary>
    /// Notifies the assignees about a deadline change after <see cref="DeadlineChangeDelay"/>.
    /// Further changes within that time restart the delay and are reported together in one message;
    /// if the deadline ends up where it started, nothing is sent.
    /// </summary>
    public void ScheduleDeadlineChanged(Guid taskId, DateTime previousUtcDueDate)
    {
        CancellationToken token;

        lock (_pendingLock)
        {
            if (_pendingDeadlineChanges.TryGetValue(taskId, out var pending))
            {
                pending.Cts.Cancel();
                pending.Cts.Dispose();
                pending.Cts = new CancellationTokenSource();
            }
            else
            {
                pending = new PendingDeadlineChange(previousUtcDueDate);
                _pendingDeadlineChanges[taskId] = pending;
            }

            token = pending.Cts.Token;
        }

        _ = Task.Run(() => SendDeadlineChangedAfterDelayAsync(taskId, token));
    }

    private async Task SendDeadlineChangedAfterDelayAsync(Guid taskId, CancellationToken token)
    {
        try
        {
            await Task.Delay(DeadlineChangeDelay, token);
        }
        catch (OperationCanceledException)
        {
            return; // A newer change took over.
        }

        PendingDeadlineChange? pending;
        lock (_pendingLock)
        {
            if (token.IsCancellationRequested || !_pendingDeadlineChanges.Remove(taskId, out pending))
                return;
        }

        try
        {
            // Read the task now, so the message shows the final deadline and the current assignees.
            var task = await _repository.GetByIdAsync(taskId);
            if (task is null || task.State == TaskState.ARCHIVED)
                return;

            var days = (int)Math.Round((task.TaskDuration.UtcDueDate - pending.OriginalUtcDueDate).TotalDays);
            if (days == 0)
                return;

            var direction = days > 0
                ? $"moved **{days} {Days(days)} later**"
                : $"moved **{-days} {Days(days)} earlier**";

            await SendToUsersAsync(task.PendingAssigneeIDs,
                "📅 **Deadline changed!**\n\n" +
                $"**{task.Description}**\n\n" +
                $"The deadline was {direction}.\n" +
                $"New deadline: {DiscordUtility.GetDiscordTimestamp(task.TaskDuration.UtcDueDate, 'f')} " +
                $"({DiscordUtility.GetDiscordTimestamp(task.TaskDuration.UtcDueDate, 'R')})\n" +
                LocationLine(task));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to send deadline change notification for task {TaskId}", taskId);
        }
    }

    private async Task<DmResult> SendToUsersAsync(IEnumerable<ulong> userIds, string text)
    {
        var failed = new List<ulong>();
        var delivered = 0;
        var transient = false;

        foreach (var userId in userIds.Distinct())
        {
            switch (await SendToUserAsync(userId, text))
            {
                case DmOutcome.Delivered: delivered++; break;
                case DmOutcome.Transient: transient = true; failed.Add(userId); break;
                default: failed.Add(userId); break;
            }
        }

        return new DmResult(failed, delivered == 0 && transient);
    }

    private async Task<DmOutcome> SendToUserAsync(ulong userId, string text)
    {
        try
        {
            // Cache first, REST fallback - works for members who are not cached.
            var user = await _client.GetUserAsync(userId);
            if (user is null)
            {
                _logger.LogWarning("Could not DM user {UserId}: user not found", userId);
                return DmOutcome.Undeliverable;
            }

            await user.SendMessageAsync(text);
            return DmOutcome.Delivered;
        }
        catch (HttpException ex) when (ex.DiscordCode == DiscordErrorCode.CannotSendMessageToUser)
        {
            _logger.LogWarning("Could not DM user {UserId}: DMs are closed", userId);
            return DmOutcome.Undeliverable;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Could not DM user {UserId}", userId);
            return DmOutcome.Transient;
        }
    }

    private string LocationLine(TaskItem task)
        => task.MessageId != 0 && task.ChannelId != 0
            ? $"📍 [Open the task](https://discord.com/channels/{_options.GuildId}/{task.ChannelId}/{task.MessageId})"
            : "📍 Check the tasks channel for complete information.";

    private static string Days(int days) => Math.Abs(days) == 1 ? "day" : "days";
}
