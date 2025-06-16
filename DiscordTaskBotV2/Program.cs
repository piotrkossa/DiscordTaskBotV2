using System;
using Discord;
using Discord.WebSocket;
using DiscordTaskBot.Core;
using DiscordTaskBot.Services;
using Microsoft.Extensions.DependencyInjection;

public class Program
{
    public static async Task Main(string[] args)
    {
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
            .AddSingleton<TaskService>()
            .BuildServiceProvider();


        var bot = services.GetRequiredService<Bot>();

        var updateService = services.GetRequiredService<DailyUpdateService>();
        _ = updateService.ScheduleDailyUpdateAsync();



        await bot.RunAsync();
    }
}