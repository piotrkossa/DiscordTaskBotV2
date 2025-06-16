using DiscordTaskBot.Helpers;
using Quartz;
using Quartz.Impl;

namespace DiscordTaskBot.Services
{
    public class DailyUpdateService : IJob
    {
        private readonly DiscordService _discordService;
        private readonly TaskService _taskService;
        private readonly ReminderService _reminderService;

        private readonly IScheduler _scheduler;

        public DailyUpdateService(DiscordService discordService, TaskService taskService, ReminderService reminderService)
        {
            _discordService = discordService;
            _taskService = taskService;
            _reminderService = reminderService;


            var factory = new StdSchedulerFactory();
            _scheduler = factory.GetScheduler().Result;

            // Rejestrujemy job i triggery od razu
            ScheduleJobsAsync().GetAwaiter().GetResult();

            // Startujemy scheduler
            _scheduler.Start().GetAwaiter().GetResult();
        }

        public async Task Execute(IJobExecutionContext context)
        {
            var trigger = context.Trigger.Key.Name;

            if (trigger == "updateMessages")
            {
                await UpdateMessages();
            }
            else if (trigger == "sendReminders")
            {
                await SendReminders();
            }
        }

        public async Task ScheduleJobsAsync()
        {
            IJobDetail job = JobBuilder.Create<DailyUpdateService>()
                .WithIdentity("dailyUpdateJob")
                .Build();

            ITrigger midnightTrigger = TriggerBuilder.Create()
                .WithIdentity("updateMessages")
                .WithSchedule(CronScheduleBuilder.DailyAtHourAndMinute(0, 1))
                .Build();

            ITrigger morningTrigger = TriggerBuilder.Create()
                .WithIdentity("sendReminders")
                .WithSchedule(CronScheduleBuilder.DailyAtHourAndMinute(9, 0))
                .Build();
                
            await _scheduler.ScheduleJob(job, new[] { midnightTrigger, morningTrigger }, replace: true);
        }

        private async Task UpdateMessages()
        {
            foreach (var task in await _taskService.GetAllTasksAsync())
            {
                (var embed, var components) = BuilderHelper.BuildMessage(task.Value, task.Key);
                var message = await _discordService.GetMessageAsync(task.Value.MessageID, task.Value.ChannelID);
                if (message != null)
                    await _discordService.UpdateMessageAsync(embed, components, message);
                else
                    await _taskService.RemoveTask(task.Key);
            }
            Console.WriteLine("Tasks updated!");
        }

        private async Task SendReminders()
        {
            foreach (var task in await _taskService.GetAllTasksAsync())
            {
                var taskData = task.Value;

                var daysLeft = (taskData.CompletionDate - DateTime.Now.Date).Days;

                if (daysLeft <= 2)
                {
                    await _reminderService.SendReminder(taskData);
                }
            }
        }
    }
}