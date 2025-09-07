namespace DiscordTaskBot.Presentation;

using Microsoft.Extensions.Logging;

public class DiscordBotOptions
{

    public string Token { get; set; } = string.Empty;
    public ulong GuildId { get; set; } = 0;
    public ulong ArchiveChannelId { get; set; } = 0;
    public bool RegisterCommandsGlobally { get; set; } = false;
    public LogLevel LogLevel { get; set; } = LogLevel.Information;
}