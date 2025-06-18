using Quartz;

namespace DiscordTaskBot.Services.Jobs
{
    public abstract class JobBase : IJob
    {
        private JobExecutionTrackerService _jobExecutionTrackerService;

        public JobBase(JobExecutionTrackerService jobExecutionTrackerService)
        {
            _jobExecutionTrackerService = jobExecutionTrackerService;
        }

        public async Task Execute(IJobExecutionContext context)
        {
            await ExecuteInternal(context);
            _jobExecutionTrackerService.MarkAsRun(context.JobDetail.Key.Name);
        }

        public abstract Task ExecuteInternal(IJobExecutionContext? context);
    }
}