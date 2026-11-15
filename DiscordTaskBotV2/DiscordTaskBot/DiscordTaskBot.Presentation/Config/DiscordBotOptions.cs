namespace DiscordTaskBot.Presentation;

using Microsoft.Extensions.Logging;

public class DiscordBotOptions
{
    public string Token { get; set; } = string.Empty;
    public ulong GuildId { get; set; } = 0;
    public ulong ArchiveChannelId { get; set; } = 0;
    public bool RegisterCommandsGlobally { get; set; } = false;
    public LogLevel LogLevel { get; set; } = LogLevel.Information;
    public ulong TaskManagamentRoleId { get; set; } = 0;

    // Reminders are only sent from ReminderStartHour:00 until ReminderEndHour:00 (local time of the machine
    // running the bot). A reminder that falls outside this window waits until ReminderStartHour.
    // 0 and 24 allow reminders at any time.
    public int ReminderStartHour { get; set; } = 8;
    public int ReminderEndHour { get; set; } = 22;}