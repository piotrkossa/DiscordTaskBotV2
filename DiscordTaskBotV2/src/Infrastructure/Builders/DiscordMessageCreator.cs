namespace DiscordTaskBot.Infrastructure;

using System;
using System.Threading.Tasks;
using Discord;
using Discord.WebSocket;
using DiscordTaskBot.Core;

/*
public class DiscordMessageCreator(DiscordSocketClient client) : IDiscordMessageCreator
{
    private readonly DiscordSocketClient _client = client;

    private EmbedBuilder _embedBuilder = new EmbedBuilder();
    private ComponentBuilder _componentBuilder = new ComponentBuilder();

    public static string CreateTimestamp(DateTime utcDateTime)
    {
        long unixTime = ((DateTimeOffset)utcDateTime).ToUnixTimeSeconds();
        return $"<t:{unixTime}:{'R'}>";
    }

    public IDiscordMessageCreator AddButton(string label, string customID, ButtonType buttonType)
    {
        _componentBuilder.WithButton(label, customID, GetButtonStyle(buttonType));
        return this;
    }

    public IDiscordMessageCreator AddField(string name, string value, bool inline = true)
    {
        _embedBuilder.AddField(name, value, inline);
        return this;
    }

    public IDiscordMessageCreator SetColor(uint colorHex)
    {
        _embedBuilder.WithColor(new Color(colorHex));
        return this;
    }

    public IDiscordMessageCreator SetDescription(string description)
    {
        _embedBuilder.WithDescription(description);
        return this;
    }

    public IDiscordMessageCreator SetTitle(string title)
    {
        _embedBuilder.WithTitle(title);
        return this;
    }

    public void Reset()
    {
        _embedBuilder = new EmbedBuilder();
        _componentBuilder = new ComponentBuilder();
    }

    public async Task<TaskLocation> SendMessage(ulong channelID)
    {
        var channel = (ITextChannel)await _client.GetChannelAsync(channelID) ?? throw new InfrastructureException($"Channel of ID: {channelID} was not found");
        var message = await channel.SendMessageAsync(
            embed: _embedBuilder.Build(),
            components: _componentBuilder.Build()
        );

        return new TaskLocation(message.Channel.Id, message.Id);
    }


    private static ButtonStyle GetButtonStyle(ButtonType buttonType)
    {
        return buttonType switch
        {
            ButtonType.Blue => ButtonStyle.Primary,
            ButtonType.Gray => ButtonStyle.Secondary,
            ButtonType.Green => ButtonStyle.Success,
            ButtonType.Red => ButtonStyle.Danger,
            _ => throw new InfrastructureException($"Button type: {buttonType} is not implemented")
        };
    }
}

*/