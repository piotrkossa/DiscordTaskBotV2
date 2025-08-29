namespace DiscordTaskBot.Core;

public interface IJobScheduler
{
    // schedules new job attached to specified taskID
    public void ScheduleJob(string taskID, DateTime runAt, Action action);

    // cancels all jobs for specified taskID
    public void CancelAllJobs(string taskID);
}