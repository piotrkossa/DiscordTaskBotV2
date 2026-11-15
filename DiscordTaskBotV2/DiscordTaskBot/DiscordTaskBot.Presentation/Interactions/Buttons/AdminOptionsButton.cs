namespace DiscordTaskBot.Presentation;

using Discord;
using Discord.Interactions;
using Discord.Net;
using Discord.WebSocket;
using DiscordTaskBot.Application;
using MediatR;
using Microsoft.Extensions.Logging;


public static class AdminTaskOptions
{
    public const string Delete = "button_taskdelete";
    public const string AddTime = "button_taskaddtime";
}


public class AdminOptionsButton(IMediator mediator, ILogger<AdminOptionsButton> logger, DiscordSocketClient client) : BaseCommand(mediator, logger)
{
    private const string ActionSelectId = "task-action-select";

    private readonly DiscordSocketClient _client = client;

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

            menuBuilder.AddOption("Add Time", AdminTaskOptions.AddTime, "Open window to add time.");
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
                    await RespondAsync($"You selected to add time to task {taskId}.", ephemeral: true);
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

    private async Task DeleteTask(string taskId, string messageId, string channelId)
    {
        await DeferAsync(ephemeral: true);

        if (!Guid.TryParse(taskId, out var taskGuid)) throw new ArgumentException($"Invalid task Id {taskId}");
        if (!ulong.TryParse(messageId, out var taskMessageId)) throw new ArgumentException($"Invalid message Id {messageId}");
        if (!ulong.TryParse(channelId, out var taskChannelId)) throw new ArgumentException($"Invalid channel Id {channelId}");

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
}
