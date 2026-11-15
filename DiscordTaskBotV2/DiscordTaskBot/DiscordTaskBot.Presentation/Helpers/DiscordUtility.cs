namespace DiscordTaskBot.Presentation;

public static class DiscordUtility
{
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
