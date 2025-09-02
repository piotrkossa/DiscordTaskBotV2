namespace DiscordTaskBot.Presentation;

using Discord.Interactions;
using Discord.WebSocket;
using MediatR;
using Microsoft.Extensions.Logging;

public class DeleteTaskButton(IMediator mediator, ILogger<DeleteTaskButton> logger) : BaseCommand(mediator, logger)
{
    [ComponentInteraction(ButtonActions.TaskDelete + ":*")]
    public async Task DeleteTask()
    {
        
    }
}