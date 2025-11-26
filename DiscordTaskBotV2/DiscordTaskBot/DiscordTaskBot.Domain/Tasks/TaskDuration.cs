namespace DiscordTaskBot.Domain;

public record TaskDuration
{
    public DateTime UtcCreationDate { get; }
    public DateTime UtcDueDate { get; private set; }

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

    public void Reschedule(DateTime newUtcDueDate)
    {  
        UtcDueDate = newUtcDueDate;
    }
    
    public void ExtendBy(TimeSpan extension)
    {
        UtcDueDate = UtcDueDate.Add(extension);
    }
    
    public void ExtendByDays(int days)
    {
        UtcDueDate = UtcDueDate.AddDays(days);
    }
}