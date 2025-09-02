namespace DiscordTaskBot.Presentation;

public static class DiscordUtility
{
    private const char SEPARATOR = ':';

    public static string CreateButtonID(DiscordButtonAction action, string taskID)
    {
        if (string.IsNullOrEmpty(taskID))
            throw new ArgumentException($"Task ID cannot be null or empty: {taskID}");

        return $"{action}{SEPARATOR}{taskID}";
    }

    public static bool TryParseButtonID(string customID, out DiscordButtonAction action, out string taskID)
    {
        action = default;
        taskID = "";

        var parts = customID?.Split(SEPARATOR);
        if (parts?.Length != 2) return false;

        return Enum.TryParse(parts[0], out action) && !string.IsNullOrEmpty(parts[1]);
    }

    public static string GetDiscordTimestamp(DateTime utcDateTime)
    {
        long unixTime = ((DateTimeOffset)utcDateTime).ToUnixTimeSeconds();
        return $"<t:{unixTime}:{'R'}>";
    }
}