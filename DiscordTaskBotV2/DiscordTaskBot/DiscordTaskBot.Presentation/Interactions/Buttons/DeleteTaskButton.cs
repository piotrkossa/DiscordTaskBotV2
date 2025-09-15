namespace DiscordTaskBot.Presentation;

using Discord.Interactions;
using Discord.WebSocket;
using DiscordTaskBot.Application;
using MediatR;
using Microsoft.Extensions.Logging;

public class DeleteTaskButton(IMediator mediator, ILogger<DeleteTaskButton> logger) : BaseCommand(mediator, logger)
{
    [ComponentInteraction(ButtonActions.TaskDelete + ":*")]
    public async Task DeleteTask(string taskId)
    {
        await base.ExecuteWithHandlingAsync(async () =>
        {
            if (!Guid.TryParse(taskId, out var result)) throw new ArgumentException($"Invalid task Id {taskId}");

            await base._mediator.Send(new DeleteTaskCommand(result, Context.User.Id));

            var component = (SocketMessageComponent)Context.Interaction;
            await component.Message.DeleteAsync();

            await FollowupAsync(ephemeral:true);
        });
    }
}