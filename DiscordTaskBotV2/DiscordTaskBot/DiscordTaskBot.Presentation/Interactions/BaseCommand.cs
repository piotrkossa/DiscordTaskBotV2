namespace DiscordTaskBot.Presentation;

using Discord;
using Discord.Interactions;
using Discord.Net;
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
            await TryNotifyUserAsync(domainException.Message);
        }
        catch (InfrastructureException infrastructureException)
        {
            _logger.LogError(infrastructureException, "[INFRASTRUCTURE EXCEPTION]: {Message}", infrastructureException.Message);
            await TryNotifyUserAsync("A technical error occurred. Please try again later.");
        }
        catch (Exception exception)
        {
            _logger.LogError(exception, "[UNRESOLVED EXCEPTION]: {Message}", exception.Message);
            await TryNotifyUserAsync("An unexpected error occurred. The administrator has been notified.");
        }
    }

    // Sending the error message can fail too (expired interaction, network error),
    // and that must not escape the handler.
    private async Task TryNotifyUserAsync(string content)
    {
        try
        {
            await FollowupOrRespond(msg => msg.Content = content);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to send error message to user {UserId}", Context.User.Id);
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

    protected static bool IsNotFound(HttpException ex)
        => ex.HttpCode == System.Net.HttpStatusCode.NotFound;
}
