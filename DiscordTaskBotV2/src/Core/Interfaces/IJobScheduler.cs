namespace DiscordTaskBot.Core;

public interface IJobScheduler
{
    public void ScheduleJob(string taskID, DateTime runAt, Action action);
    public void CancelAllJobs(string taskID);
}