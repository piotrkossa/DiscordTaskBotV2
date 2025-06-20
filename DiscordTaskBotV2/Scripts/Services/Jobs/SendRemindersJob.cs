using Quartz;

namespace DiscordTaskBot.Services.Jobs
{
    public class SendRemindersJob : JobBase
    {
        private readonly TaskService _taskService;
        private readonly ReminderService _reminderService;

        public SendRemindersJob(TaskService taskService, ReminderService reminderService, JobExecutionTrackerService jobExecService) : base(jobExecService)
        {
            _taskService = taskService;
            _reminderService = reminderService;
        }


        public override async Task ExecuteInternal(IJobExecutionContext? context)
        {
            foreach (var task in await _taskService.GetAllTasksAsync())
            {
                var taskData = task.Value;

                var daysLeft = (taskData.CompletionDate - DateTime.Now.Date).Days;

                if (daysLeft <= 2 && taskData.State < Models.TaskStates.COMPLETED)
                {
                    await _reminderService.SendReminder(taskData);
                }
            }

            Console.WriteLine("Reminders sent!");
        }
    }
}