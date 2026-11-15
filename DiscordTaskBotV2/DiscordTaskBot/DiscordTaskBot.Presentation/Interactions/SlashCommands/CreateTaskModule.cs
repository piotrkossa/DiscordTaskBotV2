namespace DiscordTaskBot.Presentation;

using System.Globalization;
using DiscordTaskBot.Application;
using Discord;
using Discord.Interactions;
using MediatR;
using DiscordTaskBot.Domain;
using Microsoft.Extensions.Logging;

public class CreateTaskModal : IModal
{
    public string Title => "New Task";

    [InputLabel("Description")]
    [ModalTextInput("description", TextInputStyle.Paragraph, placeholder: "What needs to be done?", maxLength: TaskItem.MaxDescriptionLength)]
    public string Description { get; set; } = string.Empty;

    [InputLabel("Assignees", $"Pick 1-5 people")]
    [ModalUserSelect("assignees", minValues: 1, maxValues: TaskItem.MaxAssignees)]
    public IUser[] Assignees { get; set; } = [];

    [InputLabel("Days to deadline", "A whole number from 1 to 365")]
    [ModalTextInput("days", placeholder: "e.g. 7", maxLength: 3)]
    public string Days { get; set; } = string.Empty;
}

public class CreateTaskModule(
    IMediator mediator,
    ILogger<CreateTaskModule> logger,
    ITaskRepository repository,
    IAuthorizationService authorizationService,
    TaskNotificationService notifier) : BaseCommand(mediator, logger)
{
    private const string CreateTaskModalId = "createtask-modal";
    private const int MinDays = 1;
    private const int MaxDays = 365;

    private readonly ITaskRepository _repository = repository;
    private readonly IAuthorizationService _authorizationService = authorizationService;
    private readonly TaskNotificationService _notifier = notifier;

    [SlashCommand("createtask", "Creates new task")]
    public async Task CreateTask()
    {
        await base.ExecuteWithHandlingAsync(async () =>
        {
            // Check up front so nobody fills in the form for nothing; the command handler checks again on submit.
            if (!_authorizationService.CanCreateTasksAsync(Context.User.Id))
                throw new DomainException("You do not have permission to create tasks");

            await RespondWithModalAsync<CreateTaskModal>(CreateTaskModalId);
        });
    }

    [ModalInteraction(CreateTaskModalId)]
    public async Task HandleCreateTaskModal(CreateTaskModal modal)
    {
        await base.ExecuteWithHandlingAsync(async () =>
        {
            if (!int.TryParse(modal.Days?.Trim(), NumberStyles.None, CultureInfo.InvariantCulture, out var days) || days < MinDays || days > MaxDays)
                throw new DomainException($"Days to deadline must be a whole number from {MinDays} to {MaxDays}.");

            var assignees = (modal.Assignees ?? []).DistinctBy(u => u.Id).ToList();

            // Bots cannot receive DMs and cannot work on tasks.
            if (assignees.Any(u => u.IsBot))
                throw new DomainException("Bots cannot be assigned to tasks.");

            var utcNow = DateTime.UtcNow;

            var taskItem = await base._mediator.Send(new CreateTaskCommand(
                modal.Description,
                new TaskDuration(utcNow, utcNow.AddDays(days)),
                assignees.Select(u => u.Id).ToList(),
                Context.User.Id));

            // Nothing has responded yet, so the task message is the (public) response to the form.
            await FollowupOrRespond(new DiscordTaskMessageDirector(taskItem).BuildNotStarted(), false);

            await SaveMessageLocationAsync(taskItem);

            // The task is already created and posted, so DM problems are only reported, never thrown.
            var result = await _notifier.SendNewTaskAsync(taskItem);
            if (!result.AllDelivered)
                await TryFollowupEphemeralAsync($"Task created, but {result.FailedMentions} could not be notified by DM (they may have DMs disabled).");
        });
    }

    // Remembers which message shows the task, so DMs can link to it.
    private async Task SaveMessageLocationAsync(TaskItem taskItem)
    {
        try
        {
            var message = await GetOriginalResponseAsync();
            var channelId = Context.Interaction.ChannelId ?? message.Channel?.Id ?? 0;
            if (channelId == 0)
                return;

            await _repository.SetMessageLocationAsync(taskItem.Id, channelId, message.Id);
            taskItem.AttachMessage(channelId, message.Id);
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Could not save the message location of task {TaskId}", taskItem.Id);
        }
    }
}
