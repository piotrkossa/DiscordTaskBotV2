namespace DiscordTaskBot.Presentation;

using Discord.Interactions;
using Discord.WebSocket;
using DiscordTaskBot.Application;
using MediatR;
using Microsoft.Extensions.Logging;

public class RaiseStateTaskButton(IMediator mediator, ILogger<RaiseStateTaskButton> logger) : BaseCommand(mediator, logger)
{
    [ComponentInteraction(ButtonActions.TaskRaiseState + ":*")]
    public async Task RaiseTaskState(string taskId)
    {
        await base.ExecuteWithHandlingAsync(async () =>
        {
            await DeferAsync();

            if (!Guid.TryParse(taskId, out var result)) throw new ArgumentException($"Invalid task Id {taskId}");

            var taskItem = await base._mediator.Send(new RaiseTaskStateCommand(result, Context.User.Id));

            var component = (SocketMessageComponent)Context.Interaction;
            await component.Message.ModifyAsync(msg => new DiscordTaskMessageDirector(taskItem).BuildCompletedTaskMessage());
        });
    }
}