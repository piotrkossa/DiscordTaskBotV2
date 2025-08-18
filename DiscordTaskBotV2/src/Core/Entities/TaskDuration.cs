namespace DiscordTaskBot.Core;

public class TaskDuration
{
    public TimeZoneInfo TimeZone { get; private set; }

    public DateTime CreationDate { get; }
    public DateTime DueDate { get; private set; }

    private TaskDuration() { }

    public TaskDuration(DateTime utcCreationDate, DateTime utcDueDate, TimeZoneInfo timeZone)
    {
        TimeZone = timeZone;

        CreationDate = GetLocalDateFromUtc(utcCreationDate, TimeZone);

        DueDate = GetLocalDateFromUtc(utcDueDate, TimeZone);
    }


    public int GetRemainingDays(DateTime currentUtcTime)
    {
        return Math.Max(0, (DueDate - GetLocalDateFromUtc(currentUtcTime, TimeZone)).Days);
    }

    public int GetOverdueDays(DateTime currentUtcTime)
    {
        return Math.Max(0, (GetLocalDateFromUtc(currentUtcTime, TimeZone) - DueDate).Days);
    }

    public void Reschedule(DateTime newDate)
    {
        DueDate = newDate;
    }

    public void Reschedule(int daysToAdd)
    {
        DueDate = DueDate.AddDays(daysToAdd);
    }

    private static DateTime GetLocalDateFromUtc(DateTime currentUtcTime, TimeZoneInfo timeZone)
    {
        return TimeZoneInfo.ConvertTimeFromUtc(currentUtcTime, timeZone);
    }
}
