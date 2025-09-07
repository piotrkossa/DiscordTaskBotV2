namespace DiscordTaskBot.Presentation;

using Discord;
using Discord.WebSocket;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

public class DiscordBotService
{
    private readonly DiscordSocketClient _client;
    private readonly ILogger<DiscordBotService> _logger;
    private readonly DiscordBotOptions _options;
    

    private readonly List<IDiscordEventHandler> _eventHandlers;

    public DiscordBotService(
        DiscordSocketClient client,
        ILogger<DiscordBotService> logger,
        IOptions<DiscordBotOptions> options,
        IEnumerable<IDiscordEventHandler> eventHandlers)
    {
        _client = client;
        _logger = logger;
        _options = options.Value;
        _eventHandlers = eventHandlers.ToList();

        RegisterEventHandlers();
    }


    public async Task StartAsync()
    {
        try
        {
            ValidateConfiguration();
            
            await LoginAndStartAsync();
            
            _logger.LogInformation("Discord bot started successfully");
        }
        catch (Exception ex)
        {
            _logger.LogCritical(ex, "Failed to start Discord bot");
            throw;
        }
    }

    public async Task StopAsync()
    {
        try
        {
            await _client.StopAsync();
            await _client.LogoutAsync();
            _logger.LogInformation("Discord bot stopped successfully");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error occurred while stopping Discord bot");
        }
    }


    private void RegisterEventHandlers()
    {
        foreach (var handler in _eventHandlers)
        {
            handler.Initialize();
        }

        _logger.LogDebug("Registered {Count} custom event handlers", _eventHandlers.Count);
    }

    private async Task LoginAndStartAsync()
    {
        await _client.LoginAsync(TokenType.Bot, _options.Token);
        await _client.StartAsync();
    }

    private void ValidateConfiguration()
    {
        var errors = new List<string>();

        if (string.IsNullOrWhiteSpace(_options.Token))
            errors.Add("Discord Bot Token is not specified");

        if (_options.GuildId == 0)
            errors.Add("Guild ID is not specified or invalid");

        if (_options.ArchiveChannelId == 0)
            errors.Add("Archive Channel ID is not specified or invalid");

        if (errors.Any())
        {
            var errorMessage = string.Join(Environment.NewLine, errors);
            _logger.LogCritical("Configuration validation failed:{NewLine}{Errors}", Environment.NewLine, errorMessage);
            throw new InvalidOperationException($"Invalid Discord Bot configuration: {errorMessage}");
        }

        _logger.LogDebug("Discord Bot configuration validated successfully");
    }
}