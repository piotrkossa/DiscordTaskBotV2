namespace DiscordTaskBot.Core;

public class AppearanceConfig
{
    public TimeZoneInfo TimeZone { get; set; } = TimeZoneInfo.Local;

    public uint AlertColor { get; set; } = 0xff0000;
    public string AlertEmoji { get; set; } = "❗";

    public uint DeadlineColor { get; set; } = 0x000000;
    public string DeadlineEmoji { get; set; } = "💀";

    public uint NotStartedColor { get; set; } = 0x0099ff;
    public uint InProgressColor { get; set; } = 0xffff00;
    public uint CompletedColor { get; set; } = 0x00ff00;
    public uint ArchivedColor { get; set; } = 0x00ff00;

    public string NotStartedEmoji { get; set; } = "⏳";
    public string InProgressEmoji { get; set; } = "🔨";
    public string CompletedEmoji { get; set; } = "✅";
    public string ArchivedEmoji { get; set; } = "📦";
}