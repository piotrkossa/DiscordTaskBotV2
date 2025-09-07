namespace DiscordTaskBot.Presentation;

using Discord;
using Discord.Interactions;
using DiscordTaskBot.Domain;
using DiscordTaskBot.Infrastructure;
using MediatR;
using Microsoft.Extensions.Logging;

public abstract class BaseCommand(IMediator mediator, ILogger logger) : InteractionModuleBase<SocketInteractionContext>
{
    protected readonly IMediator _mediator = mediator;
    protected readonly ILogger _logger = logger;

    protected async Task ExecuteWithHandlingAsync(Func<Task> action)
    {
        try
        {
            await DeferAsync();

            await action();
        }
        catch (DomainException domainException)
        {
            await Followup(msg =>
            {
                msg.Content = domainException.Message;
            });
        }
        catch (InfrastructureException infrastructureException)
        {
            _logger.LogError("[INFRASTRUCTURE EXCEPTION]: " + infrastructureException.Message);

            await Followup(msg =>
            {
                msg.Content = "A technical error occurred. Please try again later.";
            });
        }
        catch (Exception exception)
        {
            _logger.LogError("[UNRESOLVED EXCEPTION]: " + exception.Message);

            await Followup(msg =>
            {
                msg.Content = "An unexpected error occurred. The administrator has been notified.";
            });
        }
    }

    private async Task Followup(Action<MessageProperties> configureMessage, bool ephemeral = true)
    {
        var messageProperties = new MessageProperties();
        configureMessage(messageProperties);

        await Context.Interaction.FollowupAsync(
            text: messageProperties.Content.GetValueOrDefault(),
            embeds: messageProperties.Embeds.GetValueOrDefault(),
            components: messageProperties.Components.GetValueOrDefault(),
            allowedMentions: messageProperties.AllowedMentions.GetValueOrDefault(),
            ephemeral: ephemeral);
    }
}
