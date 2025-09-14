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
            if (!Guid.TryParse(taskId, out var result)) throw new ArgumentException($"Invalid task Id {taskId}");

            var taskItem = await base._mediator.Send(new RaiseTaskStateCommand(result, Context.User.Id));

            var component = (SocketMessageComponent)Context.Interaction;

            if (taskItem.State != Domain.TaskState.ARCHIVED)
            {
                await component.Message.ModifyAsync(new DiscordTaskMessageDirector(taskItem).BuildByState(taskItem.State));
            }
            else
            {
                await component.Message.DeleteAsync();
                var archiveChannel = (IMessageChannel)await _client.GetChannelAsync(_options.ArchiveChannelId) ?? throw new InfrastructureException("Archive channel was not found");

                await SendToChannelAsync(archiveChannel, new DiscordTaskMessageDirector(taskItem).BuildArchived());
            }

            await FollowupAsync("Task updated successfully", ephemeral: true);
        });
    }
}