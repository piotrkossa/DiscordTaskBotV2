using System;
using Discord;
using Discord.WebSocket;
using DiscordTaskBot.Configuration;
using DiscordTaskBot.Core;
using DiscordTaskBot.Services;
using Microsoft.Extensions.DependencyInjection;

public class Program
{
    public static async Task Main(string[] args)
    {
        EnvLoader.LoadEnv(Path.Combine(AppContext.BaseDirectory, ".env"));

        var services = new ServiceCollection()
            .AddSingleton(new DiscordSocketClient(new DiscordSocketConfig
            {
                GatewayIntents = GatewayIntents.AllUnprivileged | GatewayIntents.MessageContent
            }))
            .AddSingleton<Bot>()
            .AddSingleton<ButtonHandlerService>()
            .AddSingleton<DailyUpdateService>()
            .AddSingleton<DiscordService>()
            .AddSingleton<TaskService>()
            .BuildServiceProvider();


        var bot = services.GetRequiredService<Bot>();

        await services.GetRequiredService<TaskService>().LoadTasksAsync();

        var updateService = services.GetRequiredService<DailyUpdateService>();
        _ = updateService.ScheduleDailyUpdateAsync();



        await bot.RunAsync();
    }
}