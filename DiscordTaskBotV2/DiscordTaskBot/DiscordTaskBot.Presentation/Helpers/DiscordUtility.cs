namespace DiscordTaskBot.Presentation;

public static class DiscordUtility
{
    public static string GetDiscordTimestamp(DateTime utcDateTime, char format)
    {
        long unixTime = ((DateTimeOffset)utcDateTime).ToUnixTimeSeconds();
        return $"<t:{unixTime}:{format}>";
    }
}