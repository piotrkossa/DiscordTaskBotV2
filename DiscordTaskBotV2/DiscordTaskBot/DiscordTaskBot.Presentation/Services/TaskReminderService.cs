namespace DiscordTaskBot.Presentation;

using Discord;
using Discord.WebSocket;
using DiscordTaskBot.Application;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

/// <summary>
/// Every few minutes sends the one-time reminders of unfinished tasks: once when the deadline is 2 days away
/// and once after it has passed (rules in TaskItem.GetDueReminder). Sent reminders are stored per task,
/// so restarts never repeat them, and a reminder missed while the bot was offline goes out once it is back.
/// </summary>
public class TaskReminderService(
    DiscordSocketClient client,
    ITaskRepository repository,
    TaskNotificationService notifier,
    IOptions<DiscordBotOptions> options,
    ILogger<TaskReminderService> logger) : IDiscordEventHandler
{
    private static readonly TimeSpan CheckInterval = TimeSpan.FromMinutes(5);

    private readonly DiscordSocketClient _client = client;
    private readonly ITaskRepository _repository = repository;
    private readonly TaskNotificationService _notifier = notifier;
    private readonly DiscordBotOptions _options = options.Value;
    private readonly ILogger<TaskReminderService> _logger = logger;

    private int _started;

    public void Initialize()
    {
        _client.Ready += OnReady;
    }

    private Task OnReady()
    {
        // Ready fires again after every reconnect - start the loop only once.
        if (Interlocked.Exchange(ref _started, 1) == 0)
            _ = Task.Run(RunAsync);

        return Task.CompletedTask;
    }

    private async Task RunAsync()
    {
        _logger.LogInformation("Reminders are sent between {Start}:00 and {End}:00 (local time)",
            _options.ReminderStartHour, _options.ReminderEndHour);

        using var timer = new PeriodicTimer(CheckInterval);
        do
        {
            try
            {
                await SendDueRemindersAsync(DateTime.UtcNow);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Reminder check failed");
            }
        }
        while (await timer.WaitForNextTickAsync());
    }

    private async Task SendDueRemindersAsync(DateTime utcNow)
    {
        if (_client.ConnectionState != ConnectionState.Connected)
            return;

        // Quiet hours: reminders that come due at night wait here and go out at ReminderStartHour.
        if (!IsWithinReminderHours(TimeZoneInfo.ConvertTimeFromUtc(utcNow, TimeZoneInfo.Local)))
            return;

        foreach (var task in await _repository.GetAllAsync())
        {
            if (task.GetDueReminder(utcNow) is not { } kind)
                continue;

            var result = await _notifier.SendReminderAsync(task, kind);

            // Retry only when nobody got it, so nobody ever gets the same reminder twice.
            if (result.NothingDelivered)
            {
                _logger.LogWarning("{Kind} reminder for task {TaskId} will be retried", kind, task.Id);
                continue;
            }

            await _repository.MarkReminderSentAsync(task.Id, kind);
            _logger.LogInformation("{Kind} reminder sent for task {TaskId}", kind, task.Id);
        }
    }

    public static bool IsWithinReminderHours(DateTime localTime, int startHour, int endHour)
        => localTime.Hour >= startHour && localTime.Hour < endHour;

    private bool IsWithinReminderHours(DateTime localTime)
        => IsWithinReminderHours(localTime, _options.ReminderStartHour, _options.ReminderEndHour);
}
