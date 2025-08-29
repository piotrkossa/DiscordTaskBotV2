namespace DiscordTaskBot.Presentation;

using System.Reflection;
using Discord.Interactions;
using Discord.WebSocket;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

public class OnReadyHandler(
    ILogger<OnReadyHandler> logger,
    DiscordSocketClient client,
    InteractionService interactionService,
    IOptions<DiscordBotOptions> options,
    IServiceProvider services) : IDiscordEventHandler
{
    private readonly DiscordSocketClient _client = client;
    private readonly InteractionService _interactionService = interactionService;
    private readonly DiscordBotOptions _options = options.Value;
    private readonly IServiceProvider _services = services;

    private readonly ILogger<OnReadyHandler> _logger = logger;

    public async Task OnBotReadyAsync()
    {
        _logger.LogInformation("Bot connected as {Username}#{Discriminator}", 
            _client.CurrentUser.Username, 
            _client.CurrentUser.Discriminator);
        
        await RegisterSlashCommandsAsync();
    }

    public void RegisterEvents()
    {
        return;
    }


    private async Task RegisterSlashCommandsAsync()
    {
        try
        {
            if (_interactionService.SlashCommands.Any())
            {
                _logger.LogInformation("Slash commands already loaded - skipping registration");
                return;
            }

            await _interactionService.AddModulesAsync(Assembly.GetExecutingAssembly(), _services);
            
            if (_options.RegisterCommandsGlobally)
            {
                await _interactionService.RegisterCommandsGloballyAsync();
                _logger.LogInformation("Slash commands registered globally (can take up to 1h)");
            }
            else
            {
                await _interactionService.RegisterCommandsToGuildAsync(_options.GuildId, true);
                _logger.LogInformation("Slash commands registered for guild {GuildId}", _options.GuildId);
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to register slash commands");
            throw;
        }
    }
}