namespace DiscordTaskBot.Core;

public enum ButtonType
{
    Green,
    Blue,
    Gray,
    Red
}



public interface IDiscordMessageBuilder
{
    public IDiscordMessageBuilder SetTitle(string title);

    public IDiscordMessageBuilder SetDescription(string description);

    public IDiscordMessageBuilder AddField(string name, string value, bool inline = true);

    public IDiscordMessageBuilder SetColor(uint colorHex);

    public IDiscordMessageBuilder AddButton(string label, string customID, ButtonType buttonType);

    public IDiscordMessageBuilder AddFooter(string label);

    public Task<TaskLocation> SendToChannel(ulong channelID);

    public Task SendToUser(ulong userID);

    public Task EditExistingMessage(TaskLocation location);
}
