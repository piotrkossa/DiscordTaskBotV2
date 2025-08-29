namespace DiscordTaskBot.Presentation;

using Discord;
using Discord.Interactions;
using DiscordTaskBot.Domain;
using DiscordTaskBot.Infrastructure;
using MediatR;
using Microsoft.Extensions.Logging;

public abstract class BaseModule(IMediator mediator, ILogger logger) : InteractionModuleBase<SocketInteractionContext>
{
    protected readonly IMediator _mediator = mediator;
    protected readonly ILogger _logger = logger;

    protected async Task ExecuteWithHandlingAsync(Func<Task> action)
    {
        try
        {
            await action();
        }
        catch (DomainException domainException)
        {
            await RespondOrModify(msg =>
            {
                msg.Content = domainException.Message;
            });
        }
        catch (InfrastructureException infrastructureException)
        {
            _logger.LogError("[INFRASTRUCTURE EXCEPTION]: " + infrastructureException.Message);

            await RespondOrModify(msg =>
            {
                msg.Content = "A technical error occurred. Please try again later.";
            });
        }
        catch (Exception exception)
        {
            _logger.LogError("[UNRESOLVED EXCEPTION]: " + exception.Message);

            await RespondOrModify(msg =>
            {
                msg.Content = "An unexpected error occurred. The administrator has been notified.";
            });
        }
    }

    private async Task RespondOrModify(Action<MessageProperties> configureMessage, bool ephemeral = true)
    {
        var messageProperties = new MessageProperties();
        configureMessage(messageProperties);

        if (Context.Interaction.HasResponded)
        {
            var response = await Context.Interaction.GetOriginalResponseAsync();
            await response.ModifyAsync(configureMessage);
        }
        else
        {
            await Context.Interaction.RespondAsync(
                text: messageProperties.Content.GetValueOrDefault(),
                embeds: messageProperties.Embeds.GetValueOrDefault(),
                components: messageProperties.Components.GetValueOrDefault(),
                allowedMentions: messageProperties.AllowedMentions.GetValueOrDefault(),
                ephemeral: ephemeral);
        }
    }
}
