namespace DiscordTaskBot.Services
{
    public class DailyUpdateService
    {
        private readonly DiscordService _discordService;
        private readonly TaskService _taskService;

        public DailyUpdateService(DiscordService discordService, TaskService taskService) {
            _discordService = discordService;
            _taskService = taskService;
        }

        public async Task ScheduleDailyUpdateAsync()
        {
            while (true)
            {
                DateTime now = DateTime.Now;
                DateTime nextRun = now.Date.AddDays(1).AddMinutes(1);

                TimeSpan delay = nextRun - now;

                await Task.Delay(delay);

                foreach (var task in await _taskService.GetAllTasksAsync())
                {
                    (var embed, var components) = BuilderService.BuildMessage(task.Value, task.Key);
                    var message = await _discordService.GetMessageAsync(task.Value.MessageID, task.Value.ChannelID);
                    if (message != null)
                        await _discordService.UpdateMessageAsync(embed, components, message);
                }

                Console.WriteLine("Tasks updated!");
            }
        }

        private 
    }
}