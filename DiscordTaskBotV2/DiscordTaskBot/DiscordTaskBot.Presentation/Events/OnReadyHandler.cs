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

    // Ready fires again after every reconnect; these keep the work from repeating
    // while still retrying registration if a previous attempt failed.
    private readonly SemaphoreSlim _lock = new(1, 1);
    private bool _modulesLoaded;
    private bool _commandsRegistered;


    public void Initialize()
    {
        _client.Ready += OnReady;
    }


    public async Task OnReady()
    {
        _logger.LogInformation("Bot connected as {Username}#{Discriminator}",
            _client.CurrentUser.Username,
            _client.CurrentUser.Discriminator);

        await RegisterSlashCommandsAsync();
    }

    private async Task RegisterSlashCommandsAsync()
    {
        await _lock.WaitAsync();
        try
        {
            if (!_modulesLoaded)
            {
                await _interactionService.AddModulesAsync(Assembly.GetExecutingAssembly(), _services);
                _modulesLoaded = true;
            }

            if (_commandsRegistered)
            {
                _logger.LogInformation("Slash commands already registered - skipping registration");
                return;
            }

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

            _commandsRegistered = true;
        }
        catch (Exception ex)
        {
            // Will be retried on the next Ready event.
            _logger.LogError(ex, "Failed to register slash commands");
        }
        finally
        {
            _lock.Release();
        }
    }
}
