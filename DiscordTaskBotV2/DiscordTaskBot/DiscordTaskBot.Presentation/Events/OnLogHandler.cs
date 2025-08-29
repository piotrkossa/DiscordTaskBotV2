namespace DiscordTaskBot.Presentation;

using Discord;
using Discord.WebSocket;
using Microsoft.Extensions.Logging;

public class OnLogHandler(ILogger<OnLogHandler> logger, DiscordSocketClient client) : IDiscordEventHandler
{
    private readonly DiscordSocketClient _client = client;
    private readonly ILogger<OnLogHandler> _logger = logger;

    public void RegisterEvents()
    {
        _client.Log += OnLog;
    }

    private Task OnLog(LogMessage log)
    {
        var logLevel = log.Severity switch
        {
            LogSeverity.Critical => LogLevel.Critical,
            LogSeverity.Error => LogLevel.Error,
            LogSeverity.Warning => LogLevel.Warning,
            LogSeverity.Info => LogLevel.Information,
            LogSeverity.Verbose => LogLevel.Debug,
            LogSeverity.Debug => LogLevel.Trace,
            _ => LogLevel.Information
        };

        _logger.Log(logLevel, log.Exception, "[Discord.NET] {Message}", log.Message);
        return Task.CompletedTask;
    }
}