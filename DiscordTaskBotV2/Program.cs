using Discord;
using Discord.Interactions;
using Discord.WebSocket;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using


using DiscordTaskBot.Presentation;
using System.Reflection;

namespace DiscordTaskBot;

public class Program
{
    public static async Task Main(string[] args)
    {
        var services = ConfigureServices();

        // Run until exit
        await Task.Delay(Timeout.Infinite);
    }

    private static ServiceProvider ConfigureServices()
    {
        return new ServiceCollection()
            .AddLogging(config =>
            {
                config.AddConsole();
                config.SetMinimumLevel(LogLevel.Debug);
            })
            .AddSingleton(x =>
            {
                var config = new DiscordSocketConfig
                {
                    GatewayIntents = GatewayIntents.Guilds | GatewayIntents.GuildMessages | GatewayIntents.MessageContent,
                    AlwaysDownloadUsers = true,
                    LogGatewayIntentWarnings = false
                };
                return new DiscordSocketClient(config);
            })
            .AddSingleton<InteractionService>()
            .AddDiscordEventHandlers()
            .BuildServiceProvider();
    }

    private static LogLevel MapLogSeverity(LogSeverity severity) =>
        severity switch
        {
            LogSeverity.Critical => LogLevel.Critical,
            LogSeverity.Error => LogLevel.Error,
            LogSeverity.Warning => LogLevel.Warning,
            LogSeverity.Info => LogLevel.Information,
            LogSeverity.Verbose => LogLevel.Debug,
            LogSeverity.Debug => LogLevel.Trace,
            _ => LogLevel.None
        };
}
