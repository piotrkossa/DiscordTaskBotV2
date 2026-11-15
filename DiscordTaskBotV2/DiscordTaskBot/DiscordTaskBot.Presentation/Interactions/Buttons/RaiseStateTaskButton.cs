namespace DiscordTaskBot.Presentation;

using Discord;
using Discord.Interactions;
using Discord.WebSocket;
using DiscordTaskBot.Application;
using DiscordTaskBot.Infrastructure;
using MediatR;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

public class RaiseStateTaskButton(IMediator mediator, ILogger<RaiseStateTaskButton> logger, IOptions<DiscordBotOptions> options, DiscordSocketClient client) : BaseCommand(mediator, logger)
{
    private readonly DiscordBotOptions _options = options.Value;
    private readonly DiscordSocketClient _client = client;

    [ComponentInteraction(ButtonActions.TaskRaiseState + ":*")]
    public async Task RaiseTaskState(string taskId)
    {
        await base.ExecuteWithHandlingAsync(async () =>
        {
            await DeferAsync(ephemeral: true);

            if (!Guid.TryParse(taskId, out var result)) throw new ArgumentException($"Invalid task Id {taskId}");

            var taskItem = await base._mediator.Send(new RaiseTaskStateCommand(result, Context.User.Id));

            var component = (SocketMessageComponent)Context.Interaction;

            if (taskItem.State != Domain.TaskState.ARCHIVED)
            {
                await component.Message.ModifyAsync(new DiscordTaskMessageDirector(taskItem).BuildByState(taskItem.State));
            }
            else
            {
                // Post to the archive first and only then remove the original message,
                // so a missing archive channel never makes the task vanish from Discord.
                var archiveChannel = await _client.GetChannelAsync(_options.ArchiveChannelId) as IMessageChannel
                    ?? throw new InfrastructureException($"Archive channel {_options.ArchiveChannelId} was not found or is not a text channel");

                await SendToChannelAsync(archiveChannel, new DiscordTaskMessageDirector(taskItem).BuildArchived());
                await component.Message.DeleteAsync();
            }
        });
    }
}
