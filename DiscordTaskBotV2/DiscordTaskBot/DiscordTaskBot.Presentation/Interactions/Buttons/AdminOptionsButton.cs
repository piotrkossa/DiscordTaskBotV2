namespace DiscordTaskBot.Presentation;

using Discord;
using Discord.Interactions;
using Discord.Net;
using Discord.WebSocket;
using DiscordTaskBot.Application;
using DiscordTaskBot.Domain;
using MediatR;
using Microsoft.Extensions.Logging;


public static class AdminTaskOptions
{
    public const string Delete = "button_taskdelete";
    public const string AddTime = "button_taskaddtime";
    public const string EditDescription = "button_taskeditdescription";
    public const string EditAssignees = "button_taskeditassignees";
}

public class AddTimeModal : IModal
{
    public string Title => "Add Time";

    [InputLabel("Days to add (negative moves it back)")]
    [ModalTextInput("days", placeholder: "e.g. 3 or -2", maxLength: 4)]
    public string Days { get; set; } = string.Empty;
}

// Only used to read the submitted value - the modal itself is built in code so it can be pre-filled.
public class EditDescriptionModal : IModal
{
    public string Title => "Edit Description";

    [InputLabel("Description")]
    [ModalTextInput(EditDescriptionModal.InputId, TextInputStyle.Paragraph, maxLength: TaskItem.MaxDescriptionLength)]
    public string Description { get; set; } = string.Empty;

    public const string InputId = "description";
}


public class AdminOptionsButton(
    IMediator mediator,
    ILogger<AdminOptionsButton> logger,
    DiscordSocketClient client,
    IAuthorizationService authorizationService,
    ITaskRepository repository,
    TaskNotificationService notifier) : BaseCommand(mediator, logger)
{
    private const string ActionSelectId = "task-action-select";
    private const string AddTimeModalId = "task-addtime-modal";
    private const string EditDescriptionModalId = "task-editdesc-modal";
    private const string AssigneesSelectId = "task-assignees-select";

    private readonly DiscordSocketClient _client = client;
    private readonly IAuthorizationService _authorizationService = authorizationService;
    private readonly ITaskRepository _repository = repository;
    private readonly TaskNotificationService _notifier = notifier;

    [ComponentInteraction(ButtonActions.TaskOptions + ":*")]
    public async Task ShowOptionsMenu(string taskId)
    {
        await base.ExecuteWithHandlingAsync(async () =>
        {
            var message = Context.Interaction as SocketMessageComponent
                ?? throw new ArgumentException("Could not find message component from context");

            var messageId = message.Message.Id;
            var channelId = message.Channel.Id;

            var menuBuilder = new SelectMenuBuilder()
                // task-action-select:taskId:messageId:channelId
                .WithCustomId($"{ActionSelectId}:{taskId}:{messageId}:{channelId}")
                .WithPlaceholder("Choose Action...");

            menuBuilder.AddOption("Add Time", AdminTaskOptions.AddTime, "Move the deadline later or earlier.");
            menuBuilder.AddOption("Edit Description", AdminTaskOptions.EditDescription, "Change the task description.");
            menuBuilder.AddOption("Edit Assignees", AdminTaskOptions.EditAssignees, "Change who is assigned to the task.");
            menuBuilder.AddOption("Delete Task", AdminTaskOptions.Delete, "Deletes task forever.");

            var component = new ComponentBuilder().WithSelectMenu(menuBuilder).Build();

            // Nothing has responded to this interaction yet, so this has to be the initial response
            // (FollowupAsync without a prior response always failed here).
            await RespondAsync("Choose Action:", components: component, ephemeral: true);
        });
    }

    // One wildcard per custom id segment: taskId, messageId, channelId.
    [ComponentInteraction(ActionSelectId + ":*:*:*")]
    public async Task HandleActionSelect(string taskId, string messageId, string channelId, string[] values)
    {
        await base.ExecuteWithHandlingAsync(async () =>
        {
            var action = values.FirstOrDefault();

            switch (action)
            {
                case AdminTaskOptions.AddTime:
                    // Check the role up front so the user is not asked to fill in the form for nothing.
                    // The command handler checks it again when the form is submitted.
                    EnsureCanEdit();

                    // task-addtime-modal:taskId:messageId:channelId
                    await RespondWithModalAsync<AddTimeModal>($"{AddTimeModalId}:{taskId}:{messageId}:{channelId}");
                    break;

                case AdminTaskOptions.EditDescription:
                    await ShowEditDescriptionModal(taskId, messageId, channelId);
                    break;

                case AdminTaskOptions.EditAssignees:
                    await ShowAssigneesSelect(taskId, messageId, channelId);
                    break;

                case AdminTaskOptions.Delete:
                    await DeleteTask(taskId, messageId, channelId);
                    break;

                default:
                    await RespondAsync("Unknown action selected.", ephemeral: true);
                    break;
            }
        });
    }

    [ModalInteraction(AddTimeModalId + ":*:*:*")]
    public async Task HandleAddTimeModal(string taskId, string messageId, string channelId, AddTimeModal modal)
    {
        await base.ExecuteWithHandlingAsync(async () =>
        {
            await DeferAsync(ephemeral: true);

            var (taskGuid, taskMessageId, taskChannelId) = ParseIds(taskId, messageId, channelId);

            // Range and "not before creation" are validated in AddTimeCommandHandler.
            if (!int.TryParse(modal.Days?.Trim(), System.Globalization.NumberStyles.AllowLeadingSign, System.Globalization.CultureInfo.InvariantCulture, out var days))
                throw new DomainException("Days must be a whole number, e.g. 3 or -2.");

            var taskItem = await base._mediator.Send(new AddTimeCommand(taskGuid, days, Context.User.Id));

            // Refresh the task message in the channel so the new deadline is visible right away.
            await RefreshTaskMessageAsync(taskItem, taskChannelId, taskMessageId);

            // Delayed and combined with further changes, so quick corrections do not spam the assignees.
            _notifier.ScheduleDeadlineChanged(taskItem.Id, taskItem.TaskDuration.UtcDueDate.AddDays(-days));

            await TryFollowupEphemeralAsync(
                $"Time added. Assignees will be notified by DM in {TaskNotificationService.DeadlineChangeDelay.TotalMinutes:0} minutes " +
                "(further changes in that time are sent as one message).");
        });
    }

    [ModalInteraction(EditDescriptionModalId + ":*:*:*")]
    public async Task HandleEditDescriptionModal(string taskId, string messageId, string channelId, EditDescriptionModal modal)
    {
        await base.ExecuteWithHandlingAsync(async () =>
        {
            await DeferAsync(ephemeral: true);

            var (taskGuid, taskMessageId, taskChannelId) = ParseIds(taskId, messageId, channelId);

            var taskItem = await base._mediator.Send(new EditDescriptionCommand(taskGuid, modal.Description ?? string.Empty, Context.User.Id));

            await RefreshTaskMessageAsync(taskItem, taskChannelId, taskMessageId);

            await TryFollowupEphemeralAsync("Description updated");
        });
    }

    // task-assignees-select:taskId:messageId:channelId - selected users arrive as the last parameter.
    [ComponentInteraction(AssigneesSelectId + ":*:*:*")]
    public async Task HandleAssigneesSelect(string taskId, string messageId, string channelId, IUser[] users)
    {
        await base.ExecuteWithHandlingAsync(async () =>
        {
            // Acknowledges the select menu; the original response is the ephemeral menu message itself.
            await DeferAsync();

            var (taskGuid, taskMessageId, taskChannelId) = ParseIds(taskId, messageId, channelId);

            if (users.Any(u => u.IsBot))
                throw new DomainException("Bots cannot be assigned to tasks.");

            var result = await base._mediator.Send(new ChangeAssigneesCommand(taskGuid, users.Select(u => u.Id).ToList(), Context.User.Id));

            await RefreshTaskMessageAsync(result.Task, taskChannelId, taskMessageId);

            // Only newly added people get a message; existing assignees already know about the task.
            var reply = "Assignees updated.";
            if (result.AddedAssigneeIds.Count > 0)
            {
                var dm = await _notifier.SendNewTaskAsync(result.Task, result.AddedAssigneeIds);
                if (!dm.AllDelivered)
                    reply += $" {dm.FailedMentions} could not be notified by DM.";
            }

            try
            {
                await ModifyOriginalResponseAsync(msg =>
                {
                    msg.Content = reply;
                    msg.Components = new ComponentBuilder().Build();
                    msg.AllowedMentions = AllowedMentions.None;
                });
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "Could not update the assignees menu");
            }
        });
    }

    private async Task ShowAssigneesSelect(string taskId, string messageId, string channelId)
    {
        EnsureCanEdit();

        if (!Guid.TryParse(taskId, out var taskGuid)) throw new ArgumentException($"Invalid task Id {taskId}");

        var taskItem = await _repository.GetByIdAsync(taskGuid) ?? throw new DomainException("Task not found");

        var menu = new SelectMenuBuilder()
            .WithType(ComponentType.UserSelect)
            .WithCustomId($"{AssigneesSelectId}:{taskId}:{messageId}:{channelId}")
            .WithPlaceholder("Pick assignees...")
            .WithMinValues(1)
            .WithMaxValues(TaskItem.MaxAssignees)
            // Current assignees are pre-selected.
            .WithDefaultValues(taskItem.AssigneeIDs
                .Select(id => new SelectMenuDefaultValue(id, SelectDefaultValueType.User))
                .ToArray());

        var component = new ComponentBuilder().WithSelectMenu(menu).Build();

        await RespondAsync($"Choose up to {TaskItem.MaxAssignees} assignees:", components: component, ephemeral: true);
    }

    private async Task ShowEditDescriptionModal(string taskId, string messageId, string channelId)
    {
        EnsureCanEdit();

        if (!Guid.TryParse(taskId, out var taskGuid)) throw new ArgumentException($"Invalid task Id {taskId}");

        var taskItem = await _repository.GetByIdAsync(taskGuid) ?? throw new DomainException("Task not found");

        // Built in code (not from EditDescriptionModal) so the current description is pre-filled.
        var modal = new ModalBuilder()
            .WithTitle("Edit Description")
            .WithCustomId($"{EditDescriptionModalId}:{taskId}:{messageId}:{channelId}")
            .AddTextInput("Description", EditDescriptionModal.InputId, TextInputStyle.Paragraph,
                maxLength: TaskItem.MaxDescriptionLength, required: true, value: taskItem.Description)
            .Build();

        await Context.Interaction.RespondWithModalAsync(modal);
    }

    private async Task DeleteTask(string taskId, string messageId, string channelId)
    {
        await DeferAsync(ephemeral: true);

        var (taskGuid, taskMessageId, taskChannelId) = ParseIds(taskId, messageId, channelId);

        await base._mediator.Send(new DeleteTaskCommand(taskGuid, Context.User.Id));

        // Delete the task message in the channel - not the ephemeral menu this interaction came from.
        if (await _client.GetChannelAsync(taskChannelId) is IMessageChannel channel)
        {
            try
            {
                await channel.DeleteMessageAsync(taskMessageId);
            }
            catch (HttpException ex) when (IsNotFound(ex))
            {
                // Message was already removed - nothing to do.
            }
        }
        else
        {
            _logger.LogWarning("Channel {ChannelId} of deleted task {TaskId} was not found", taskChannelId, taskGuid);
        }

        // Remove the ephemeral "Choose Action" menu as well - it points to a task that no longer exists.
        // (Ephemeral messages can only be deleted through the interaction, not through the channel.)
        await DeleteOriginalResponseAsync();
    }

    private void EnsureCanEdit()
    {
        if (!_authorizationService.CanEditTasksAsync(Context.User.Id))
            throw new DomainException("You do not have permission to edit tasks");
    }

    // The change is already saved at this point, so a failed refresh is only logged.
    private async Task RefreshTaskMessageAsync(TaskItem taskItem, ulong channelId, ulong messageId)
    {
        try
        {
            if (await _client.GetChannelAsync(channelId) is IMessageChannel channel)
                await channel.ModifyMessageAsync(messageId, new DiscordTaskMessageDirector(taskItem).BuildByState(taskItem.State));
            else
                _logger.LogWarning("Channel {ChannelId} of task {TaskId} was not found", channelId, taskItem.Id);
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Could not refresh message {MessageId} of task {TaskId}", messageId, taskItem.Id);
        }
    }

    private static (Guid TaskId, ulong MessageId, ulong ChannelId) ParseIds(string taskId, string messageId, string channelId)
    {
        if (!Guid.TryParse(taskId, out var taskGuid)) throw new ArgumentException($"Invalid task Id {taskId}");
        if (!ulong.TryParse(messageId, out var taskMessageId)) throw new ArgumentException($"Invalid message Id {messageId}");
        if (!ulong.TryParse(channelId, out var taskChannelId)) throw new ArgumentException($"Invalid channel Id {channelId}");
        return (taskGuid, taskMessageId, taskChannelId);
    }
}
