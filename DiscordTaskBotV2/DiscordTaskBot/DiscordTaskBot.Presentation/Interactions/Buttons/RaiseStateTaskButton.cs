namespace DiscordTaskBot.Presentation;

using Discord;
using Discord.Interactions;
using Discord.Net;
using Discord.WebSocket;
using DiscordTaskBot.Application;
using DiscordTaskBot.Domain;
using DiscordTaskBot.Infrastructure;
using MediatR;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

/// <summary>
/// Start / Complete / Archive buttons under a task message.
/// Start and Complete change the clicking assignee's own progress; the task state follows from everyone's progress.
/// </summary>
public class RaiseStateTaskButton(
    IMediator mediator,
    ILogger<RaiseStateTaskButton> logger,
    IOptions<DiscordBotOptions> options,
    DiscordSocketClient client,
    ITaskRepository repository,
    IAuthorizationService authorizationService) : BaseCommand(mediator, logger)
{
    private readonly DiscordBotOptions _options = options.Value;
    private readonly DiscordSocketClient _client = client;
    private readonly ITaskRepository _repository = repository;
    private readonly IAuthorizationService _authorizationService = authorizationService;

    [ComponentInteraction(ButtonActions.TaskStart + ":*")]
    public Task Start(string taskId) => HandleAsync(taskId, TaskProgressAction.Start);

    [ComponentInteraction(ButtonActions.TaskComplete + ":*")]
    public Task Complete(string taskId) => HandleAsync(taskId, TaskProgressAction.Complete);

    [ComponentInteraction(ButtonActions.TaskArchive + ":*")]
    public Task Archive(string taskId) => HandleAsync(taskId, TaskProgressAction.Archive);

    // Messages posted before per-person progress have one button whose meaning depends on the task state.
    [ComponentInteraction(ButtonActions.TaskRaiseState + ":*")]
    public Task LegacyRaiseState(string taskId) => HandleAsync(taskId, action: null);

    private async Task HandleAsync(string taskId, TaskProgressAction? action)
    {
        await base.ExecuteWithHandlingAsync(async () =>
        {
            await DeferAsync(ephemeral: true);

            if (!Guid.TryParse(taskId, out var id)) throw new ArgumentException($"Invalid task Id {taskId}");

            var current = await _repository.GetByIdAsync(id) ?? throw new DomainException("Task could not be found");

            action ??= current.State switch
            {
                TaskState.NOT_STARTED => TaskProgressAction.Start,
                TaskState.IN_PROGRESS => TaskProgressAction.Complete,
                _ => TaskProgressAction.Archive
            };

            TaskItem taskItem;
            if (current.State == TaskState.ARCHIVED)
            {
                if (action != TaskProgressAction.Archive)
                    throw new DomainException("The task is already archived.");

                // Archived in the database, but the message is still here - an earlier archive attempt failed
                // to post to the archive channel. Finish archiving instead of failing on the state change.
                if (!current.IsAssignee(Context.User.Id) && !_authorizationService.CanEditTasksAsync(Context.User.Id))
                    throw new DomainException("You do not have permission to update this task");

                EnsureCanPostToArchive();
                taskItem = current;
            }
            else
            {
                // Check before the state is saved, so a missing permission never leaves the task half-archived.
                if (action == TaskProgressAction.Archive)
                    EnsureCanPostToArchive();

                taskItem = await base._mediator.Send(new UpdateTaskProgressCommand(id, action.Value, Context.User.Id));
            }

            var component = (SocketMessageComponent)Context.Interaction;

            if (taskItem.State != TaskState.ARCHIVED)
            {
                await component.Message.ModifyAsync(new DiscordTaskMessageDirector(taskItem).BuildByState(taskItem.State));
                return;
            }

            // Post to the archive first and only then remove the original message,
            // so a failure never makes the task vanish from Discord. Clicking Archive again retries.
            var archiveChannel = await _client.GetChannelAsync(_options.ArchiveChannelId) as IMessageChannel
                ?? throw new InfrastructureException($"Archive channel {_options.ArchiveChannelId} was not found or is not a text channel");

            try
            {
                await SendToChannelAsync(archiveChannel, new DiscordTaskMessageDirector(taskItem).BuildArchived());
            }
            catch (HttpException ex) when (ex.DiscordCode == DiscordErrorCode.MissingPermissions || ex.HttpCode == System.Net.HttpStatusCode.Forbidden)
            {
                _logger.LogError(ex, "Missing permissions in archive channel {ChannelId}", _options.ArchiveChannelId);
                throw new DomainException(MissingArchivePermissionsMessage("required permissions"));
            }

            await component.Message.DeleteAsync();
        });
    }

    private void EnsureCanPostToArchive()
    {
        // Only checkable for cached guild channels; anything else is caught when sending.
        if (_client.GetChannel(_options.ArchiveChannelId) is not SocketGuildChannel channel)
            return;

        var missing = DiscordUtility.GetMissingArchivePermissions(channel);
        if (missing.Count > 0)
        {
            _logger.LogError("Missing permissions in archive channel {ChannelId}: {Missing}", channel.Id, string.Join(", ", missing));
            throw new DomainException(MissingArchivePermissionsMessage(string.Join(", ", missing)));
        }
    }

    private string MissingArchivePermissionsMessage(string missing)
        => $"The bot cannot post in the archive channel <#{_options.ArchiveChannelId}> (missing: {missing}). " +
           "Allow View Channel, Send Messages and Embed Links for the bot (or its role) in that channel's permission settings, then click Archive again.";
}
