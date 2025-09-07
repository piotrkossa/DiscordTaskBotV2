namespace DiscordTaskBot;

using Discord;
using Discord.Interactions;
using Discord.WebSocket;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using DiscordTaskBot.Presentation;
using DiscordTaskBot.Infrastructure;

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
            .AddInfrastructure()
            .AddPresentation()
            .BuildServiceProvider();
    }
}
