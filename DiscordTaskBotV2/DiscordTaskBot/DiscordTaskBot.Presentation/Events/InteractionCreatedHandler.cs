namespace DiscordTaskBot.Presentation;

using Discord;
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

        // Commands run in RunMode.Async (the default), so ExecuteCommandAsync returns before they finish.
        // Failures such as unknown commands, failed preconditions (e.g. RequireUserPermission) or bad arguments
        // are only reported through this event, so this is the single place that answers them.
        _interactionService.InteractionExecuted += OnInteractionExecuted;
    }

    private async Task OnInteractionCreated(SocketInteraction interaction)
    {
        try
        {
            var context = new SocketInteractionContext(_client, interaction);
            await _interactionService.ExecuteCommandAsync(context, _services);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error handling interaction {InteractionId}", interaction.Id);
        }
    }

    private async Task OnInteractionExecuted(ICommandInfo command, IInteractionContext context, IResult result)
    {
        if (result.IsSuccess)
            return;

        _logger.LogWarning("Interaction execution failed ({Command}): {Error} - {Reason}",
            command?.Name ?? "unknown", result.Error, result.ErrorReason);

        await HandleInteractionError(context.Interaction, result);
    }

    private async Task HandleInteractionError(IDiscordInteraction interaction, IResult result)
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
