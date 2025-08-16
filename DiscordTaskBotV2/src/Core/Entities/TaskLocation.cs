namespace DiscordTaskBot.Core;

public class TaskLocation(ulong channelID, ulong messageID)
{
    public ulong ChannelID { get; private set; } = channelID;
    public ulong MessageID { get; private set; } = messageID;

    public void ChangeLocation(ulong channelID, ulong messageID)
    {
        ChannelID = channelID;
        MessageID = messageID;
    }
}