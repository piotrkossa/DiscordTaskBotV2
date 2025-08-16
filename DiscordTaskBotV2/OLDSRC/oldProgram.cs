using Discord;
using Discord.WebSocket;
using DiscordTaskBot.Configuration;
using DiscordTaskBot.Core;
using DiscordTaskBot.Services;
using DiscordTaskBot.Services.Jobs;
using Microsoft.Extensions.DependencyInjection;
using Quartz;
using Microsoft.Extensions.Logging;

public class oldProgram
{
    public static async Task Main(string[] args)
    {
        EnvLoader.LoadEnv(Path.Combine(AppContext.BaseDirectory, ".env"));

        var services = new ServiceCollection()
            .AddLogging(loggingBuilder =>
            {
                loggingBuilder.AddConsole();
            })
            
            // Discord.NET client
            .AddSingleton(new DiscordSocketClient(new DiscordSocketConfig
            {
                GatewayIntents = GatewayIntents.Guilds | GatewayIntents.GuildMessages | GatewayIntents.MessageContent
            }))

            // Services
            .AddSingleton<Bot>()
            .AddSingleton<ButtonHandlerService>()
            .AddSingleton<JobExecutionTrackerService>()
            .AddSingleton<DiscordService>()
            .AddSingleton<TaskService>()
            .AddSingleton<ReminderService>()

            // Quartz jobs
            .AddTransient<SendRemindersJob>()
            .AddTransient<UpdateMessagesJob>()

            // Quartz config
            .AddQuartz(q =>
            {
                q.AddJob<UpdateMessagesJob>(opts => opts.WithIdentity("updateMessages"));
                q.AddTrigger(t => t
                    .ForJob("updateMessages")
                    .WithIdentity("updateMessagesTrigger")
                    .WithSchedule(CronScheduleBuilder.DailyAtHourAndMinute(0, 1)));

                q.AddJob<SendRemindersJob>(opts => opts.WithIdentity("sendReminders"));
                q.AddTrigger(t => t
                    .ForJob("sendReminders")
                    .WithIdentity("sendRemindersTrigger")
                    .WithSchedule(CronScheduleBuilder.DailyAtHourAndMinute(9, 0)));
            })
            .AddQuartzHostedService(options =>
            {
                options.WaitForJobsToComplete = true;
            })

            .BuildServiceProvider();


        var bot = services.GetRequiredService<Bot>();

        services.GetRequiredService<TaskService>();

        services.GetRequiredService<JobExecutionTrackerService>();

        await bot.RunAsync();
    }
}