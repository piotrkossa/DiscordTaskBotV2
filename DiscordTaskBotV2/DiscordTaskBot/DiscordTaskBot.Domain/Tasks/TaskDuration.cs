namespace DiscordTaskBot.Domain;

public record TaskDuration
{
    public DateTime UtcCreationDate { get; }
    public DateTime UtcDueDate { get; }

    public TaskDuration(DateTime utcCreationDate, DateTime utcDueDate)
    {
        UtcCreationDate = utcCreationDate;
        UtcDueDate = utcDueDate;
    }

    public DateTime LocalCreationDate(TimeZoneInfo timeZoneInfo)
        => TimeZoneInfo.ConvertTimeFromUtc(UtcCreationDate, timeZoneInfo);
    
    public DateTime LocalDueDate(TimeZoneInfo timeZoneInfo) 
        => TimeZoneInfo.ConvertTimeFromUtc(UtcDueDate, timeZoneInfo);
    
    public TimeSpan TimeUntilDue(DateTime currentUtcTime) 
        => UtcDueDate - currentUtcTime;
    
    public bool IsOverdue(DateTime currentUtcTime) 
        => currentUtcTime > UtcDueDate;
        
    public TimeSpan Duration => UtcDueDate - UtcCreationDate;

    public TaskDuration Reschedule(DateTime newUtcDueDate)
    {  
        return new TaskDuration(UtcCreationDate, newUtcDueDate);
    }
    
    public TaskDuration ExtendBy(TimeSpan extension)
    {
        return new TaskDuration(UtcCreationDate, UtcDueDate.Add(extension));
    }
    
    public TaskDuration ExtendByDays(int days)
    {
        return new TaskDuration(UtcCreationDate, UtcDueDate.AddDays(days));
    }
}