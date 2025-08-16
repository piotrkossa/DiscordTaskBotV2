namespace DiscordTaskBot.Core;

public class TaskDuration
{
    private readonly TimeZoneInfo _timeZone;

    public DateTime CreationDate { get; }
    public DateTime DueDate { get; private set; }

    public TaskDuration(DateTime utcCreationDate, DateTime utcDueDate, TimeZoneInfo timeZone)
    {
        _timeZone = timeZone;

        CreationDate = GetLocalDateFromUtc(utcCreationDate, _timeZone);

        DueDate = GetLocalDateFromUtc(utcDueDate, _timeZone);
    }


    public int GetRemainingDays(DateTime currentUtcTime)
    {
        return Math.Max(0, (DueDate - GetLocalDateFromUtc(currentUtcTime, _timeZone)).Days);
    }

    public int GetOverdueDays(DateTime currentUtcTime)
    {
        return Math.Max(0, (GetLocalDateFromUtc(currentUtcTime, _timeZone) - DueDate).Days);
    }

    public void Reschedule(DateTime newDate)
    {
        DueDate = newDate;
    }

    private static DateTime GetLocalDateFromUtc(DateTime currentUtcTime, TimeZoneInfo timeZone)
    {
        return TimeZoneInfo.ConvertTimeFromUtc(currentUtcTime, timeZone);
    }
}
