namespace DiscordTaskBot.Core;

public enum DiscordButtonAction
{
    UPPER_STATE,
    DELETE
}


public interface IDiscordMessageUtility
{
    public string CreateButtonID(DiscordButtonAction action, string taskID);
    public bool TryParseButtonID(string customID, out DiscordButtonAction action, out string taskID);

    public string GetDiscordTimestamp(DateTime utcDateTime);
    public string GetUserMention(ulong userID);
}
