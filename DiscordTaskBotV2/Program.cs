using System;
using DiscordTaskBot.Core;
using DiscordTaskBot.Services;
using Microsoft.Extensions.DependencyInjection;

public class Program
{
    public static async Task Main(string[] args)
    {
        var services = new ServiceCollection()
            .AddSingleton<Bot>()
            .AddSingleton<ButtonHandlerService>()
            //.AddSingleton<ReminderService>()
            // Dodaj inne serwisy...
            .BuildServiceProvider();


        var bot = services.GetRequiredService<Bot>();
        await bot.RunAsync();
    }
}