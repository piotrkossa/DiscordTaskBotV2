namespace DiscordTaskBot.Presentation;

using Microsoft.Extensions.Logging;

public class DiscordBotOptions
{
    public const string SectionName = "DiscordBot";

    public string Token { get; set; } = string.Empty;
    public ulong GuildId { get; set; }
    public ulong ArchiveChannelId { get; set; }
    public bool RegisterCommandsGlobally { get; set; } = false;
    public LogLevel LogLevel { get; set; } = LogLevel.Information;
}