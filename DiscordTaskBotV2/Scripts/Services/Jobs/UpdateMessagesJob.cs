using DiscordTaskBot.Helpers;
using Quartz;

namespace DiscordTaskBot.Services.Jobs
{
    public class UpdateMessagesJob : JobBase
    {
        private readonly DiscordService _discordService;
        private readonly TaskService _taskService;

        public UpdateMessagesJob(DiscordService discordService, TaskService taskService, JobExecutionTrackerService jobExecService) : base(jobExecService)
        {
            _discordService = discordService;
            _taskService = taskService;
        }

        public override async Task ExecuteInternal(IJobExecutionContext? context)
        {
            foreach (var task in _taskService.GetAllTasks())
            {
                (var embed, var components) = BuilderHelper.BuildMessage(task.Value, task.Key);
                var message = await _discordService.GetMessageAsync(task.Value.MessageID, task.Value.ChannelID);
                if (message != null)
                    await _discordService.UpdateMessageAsync(embed, components, message);
                else
                    _taskService.RemoveTask(task.Key);
            }

            Console.WriteLine("Tasks updated!");
        }
    }
}