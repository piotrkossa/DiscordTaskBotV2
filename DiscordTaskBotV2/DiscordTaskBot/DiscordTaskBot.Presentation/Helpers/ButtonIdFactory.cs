namespace DiscordTaskBot.Presentation;

public static class ButtonIdFactory
{
    private const char Separator = '|';

    public static string TaskComplete(string taskId)
        => $"task{Separator}complete{Separator}{taskId}";

    public static string TaskDelete(string taskId)
        => $"task{Separator}delete{Separator}{taskId}";

    public static string UserBan(string userId)
        => $"user{Separator}ban{Separator}{userId}";

    public static (string Domain, string Action, string Data) Parse(string customId)
    {
        var parts = customId.Split(Separator, 3);
        if (parts.Length < 2)
            throw new InvalidOperationException($"Invalid CustomId: {customId}");

        return (
            Domain: parts[0],
            Action: parts[1],
            Data: parts.Length > 2 ? parts[2] : string.Empty
        );
    }
}