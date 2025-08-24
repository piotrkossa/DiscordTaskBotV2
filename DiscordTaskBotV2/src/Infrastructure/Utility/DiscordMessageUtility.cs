namespace DiscordTaskBot.Infrastructure;

using DiscordTaskBot.Core;

public static class DiscordMessageUtility
{
    private const char SEPARATOR = '_';

    public static string CreateButtonID(DiscordButtonAction action, string taskID)
    {
        if (string.IsNullOrEmpty(taskID))
            throw new DomainException($"Task ID cannot be null or empty: {taskID}");

        if (taskID.Contains(SEPARATOR))
            throw new DomainException($"Task ID cannot contain underscore: {taskID}");

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