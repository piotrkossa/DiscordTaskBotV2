namespace DiscordTaskBot.Core;

public class TaskDuration
{
    public DateTime UtcCreationDate { get; }
    public DateTime UtcDueDate { get; private set; }

    private TaskDuration() { }

    public TaskDuration(DateTime utcCreationDate, DateTime utcDueDate)
    {
        UtcCreationDate = utcCreationDate;

        UtcDueDate = utcDueDate;
    }

    public DateTime LocalCreationDate(TimeZoneInfo timeZoneInfo) => TimeZoneInfo.ConvertTimeFromUtc(UtcCreationDate, timeZoneInfo);
    public DateTime LocalDueDate(TimeZoneInfo timeZoneInfo) => TimeZoneInfo.ConvertTimeFromUtc(UtcDueDate, timeZoneInfo);

    public TimeSpan UtcDueDateOffset(DateTime currentUtcTime)
    {
        return UtcDueDate - currentUtcTime;
    }

    public void Reschedule(DateTime newUtcDate)
    {
        UtcDueDate = newUtcDate;
    }

    public void Reschedule(int daysToAdd)
    {
        UtcDueDate = UtcDueDate.AddDays(daysToAdd);
    }
}
