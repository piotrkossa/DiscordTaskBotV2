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
            await action();
        }
        catch (DomainException domainException)
        {
            await FollowupOrRespond(msg =>
            {
                msg.Content = domainException.Message;
            });
        }
        catch (InfrastructureException infrastructureException)
        {
            _logger.LogError("[INFRASTRUCTURE EXCEPTION]: " + infrastructureException.Message);

            await FollowupOrRespond(msg =>
            {
                msg.Content = "A technical error occurred. Please try again later.";
            });
        }
        catch (Exception exception)
        {
            _logger.LogError("[UNRESOLVED EXCEPTION]: " + exception.Message);

            await FollowupOrRespond(msg =>
            {
                msg.Content = "An unexpected error occurred. The administrator has been notified.";
            });
        }
    }

    protected async Task FollowupOrRespond(Action<MessageProperties> configureMessage, bool ephemeral = true)
    {
        var messageProperties = new MessageProperties();
        configureMessage(messageProperties);

        if (Context.Interaction.HasResponded)
        {
            await FollowupAsync(
                text: messageProperties.Content.GetValueOrDefault(),
                embeds: messageProperties.Embeds.GetValueOrDefault(),
                embed: messageProperties.Embed.GetValueOrDefault(),
                components: messageProperties.Components.GetValueOrDefault(),
                allowedMentions: messageProperties.AllowedMentions.GetValueOrDefault(),
                ephemeral: ephemeral);
        }
        else
        {
            await RespondAsync(
                text: messageProperties.Content.GetValueOrDefault(),
                embeds: messageProperties.Embeds.GetValueOrDefault(),
                embed: messageProperties.Embed.GetValueOrDefault(),
                components: messageProperties.Components.GetValueOrDefault(),
                allowedMentions: messageProperties.AllowedMentions.GetValueOrDefault(),
                ephemeral: ephemeral);
        }
    }

    protected async Task SendToChannelAsync(IMessageChannel channel, Action<MessageProperties> configureMessage)
    {
        var messageProperties = new MessageProperties();
        configureMessage(messageProperties);

        await channel.SendMessageAsync(
            text: messageProperties.Content.GetValueOrDefault(),
            embeds: messageProperties.Embeds.GetValueOrDefault(),
            embed: messageProperties.Embed.GetValueOrDefault(),
            components: messageProperties.Components.GetValueOrDefault(),
            allowedMentions: messageProperties.AllowedMentions.GetValueOrDefault());
    }
}
