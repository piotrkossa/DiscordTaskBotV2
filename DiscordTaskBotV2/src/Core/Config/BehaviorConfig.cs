namespace DiscordTaskBot.Core;

public class BehaviorConfig
{
    public TimeSpan TimespanBeforeAlert { get; set; } = TimeSpan.FromHours(24);
    //public bool EnableNotifications { get; set; } = true;
    //public int NotificationHour { get; set; } = 9;

    public bool NotifyOnAssignment { get; set; } = true;
    public bool NotifyOnCompletion { get; set; } = true;

    public bool AutoArchiveCompleted { get; set; } = true;
    public uint ArchiveChannelID { get; set; } = 0;

    public TimeSpan DeadlineTolerance { get; set; } = TimeSpan.FromHours(1);
}