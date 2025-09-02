namespace DiscordTaskBot.Presentation;

using Discord.Interactions;
using Discord.WebSocket;
using Microsoft.Extensions.Logging;

public class InteractionCreatedHandler(
    ILogger<InteractionCreatedHandler> logger,
    DiscordSocketClient client,
    IServiceProvider services,
    InteractionService interactionService) : IDiscordEventHandler
{
    private readonly DiscordSocketClient _client = client;
    private readonly ILogger<InteractionCreatedHandler> _logger = logger;
    private readonly IServiceProvider _services = services;
    private readonly InteractionService _interactionService = interactionService;

    public void Initialize()
    {
        _client.InteractionCreated += OnInteractionCreated;
    }

    private async Task OnInteractionCreated(SocketInteraction interaction)
    {
        try
        {
            var context = new SocketInteractionContext(_client, interaction);
            var result = await _interactionService.ExecuteCommandAsync(context, _services);

            if (!result.IsSuccess)
            {
                _logger.LogWarning("Interaction execution failed: {Error}", result.ErrorReason);

                await HandleInteractionError(interaction, result);
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error handling interaction {InteractionId}", interaction.Id);
        }
    }
    
    private async Task HandleInteractionError(SocketInteraction interaction, IResult result)
    {
        try
        {
            var errorMessage = result.Error switch
            {
                InteractionCommandError.UnknownCommand => "Unknown command",
                InteractionCommandError.BadArgs => "Invalid command arguments",
                InteractionCommandError.Exception => "An error occurred while executing the command",
                InteractionCommandError.Unsuccessful => "The command was not executed successfully",
                _ => "An unexpected error occurred"
            };

            if (interaction.HasResponded)
            {
                await interaction.FollowupAsync(errorMessage, ephemeral: true);
            }
            else
            {
                await interaction.RespondAsync(errorMessage, ephemeral: true);
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to send error message for interaction {InteractionId}", interaction.Id);
        }
    }
}