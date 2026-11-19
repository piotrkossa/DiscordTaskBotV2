namespace DiscordTaskBot.Presentation;

public static class DiscordUtility
{
    /// <summary>Permissions the bot needs in the archive channel that it does not have there (channel overrides included).</summary>
    public static List<string> GetMissingArchivePermissions(Discord.WebSocket.SocketGuildChannel channel)
    {
        var permissions = channel.Guild.CurrentUser.GetPermissions(channel);
        var missing = new List<string>();

        if (!permissions.ViewChannel) missing.Add("View Channel");
        if (!permissions.SendMessages) missing.Add("Send Messages");
        if (!permissions.EmbedLinks) missing.Add("Embed Links");

        return missing;
    }

    public static string GetDiscordTimestamp(DateTime utcDateTime, char format)
    {
        // Dates read back from SQLite have Kind = Unspecified; casting those to DateTimeOffset
        // would treat them as local time and shift the timestamp by the machine's UTC offset.
        var utc = utcDateTime.Kind == DateTimeKind.Local
            ? utcDateTime.ToUniversalTime()
            : DateTime.SpecifyKind(utcDateTime, DateTimeKind.Utc);

        long unixTime = new DateTimeOffset(utc).ToUnixTimeSeconds();
        return $"<t:{unixTime}:{format}>";
    }
}
