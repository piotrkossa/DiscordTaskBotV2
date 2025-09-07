using Discord;
using Discord.Interactions;
using Discord.WebSocket;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using DiscordTaskBot.Presentation;
using DiscordTaskBot.Infrastructure;
using System.Reflection;

namespace DiscordTaskBot;

internal class Program
{
    private static async Task Main(string[] args)
    {
        var services = ConfigureServices();

        await services.GetRequiredService<DiscordBotService>().StartAsync();

        await Task.Delay(-1);
    }

    private static ServiceProvider ConfigureServices()
    {
        var configuration = BuildConfiguration();

        return new ServiceCollection()
            .AddLogging(ConfigureLogging)
            .AddMediator()
            .AddDiscordServices()
            .AddInfrastructure()
            .AddPresentation()
            .AddConfiguration(configuration)
            .BuildServiceProvider();
    }

    private static IConfiguration BuildConfiguration()
    {
        var configFile = Path.Combine(AppContext.BaseDirectory, "appsettings.json");

        if (!File.Exists(configFile))
        {
            var defaultConfig = new
            {
                DiscordBot = new DiscordBotOptions()
            };
            var json = System.Text.Json.JsonSerializer.Serialize(defaultConfig,
                new System.Text.Json.JsonSerializerOptions { WriteIndented = true });

            File.WriteAllText(configFile, json);
            Console.WriteLine("⚠️ appsettings.json was not found, so default was generated.");
        }

        return new ConfigurationBuilder()
                .AddJsonFile("appsettings.json", optional: false, reloadOnChange: false)
                .Build();
    }

    private static void ConfigureLogging(ILoggingBuilder logging)
    {
        logging.AddConsole();
        logging.SetMinimumLevel(LogLevel.Debug);
    }
}

internal static class ServiceCollectionExtensions
{
    public static IServiceCollection AddConfiguration(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddSingleton<IConfiguration>(configuration);
        services.Configure<DiscordBotOptions>(configuration.GetSection("DiscordBot"));
        return services;
    }

    public static IServiceCollection AddDiscordServices(this IServiceCollection services)
    {
        var config = new DiscordSocketConfig
        {
            GatewayIntents = GatewayIntents.Guilds | GatewayIntents.GuildMessages | GatewayIntents.MessageContent,
            AlwaysDownloadUsers = true,
            LogGatewayIntentWarnings = false
        };
        var client = new DiscordSocketClient(config);
        var interactionService = new InteractionService(client.Rest);

        services.AddSingleton(client);
        services.AddSingleton(interactionService);

        return services;
    }

    public static IServiceCollection AddMediator(this IServiceCollection services)
    {
        services.AddMediatR(cfg =>
        {
            cfg.RegisterServicesFromAssembly(Assembly.GetExecutingAssembly());
        });
        return services;
    }
}
