namespace DiscordTaskBot.Presentation;

using Discord.Interactions;
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

            await base._mediator.Send(new RaiseTaskStateCommand(result, Context.User.Id));

        });
    }
}