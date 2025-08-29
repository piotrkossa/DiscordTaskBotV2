namespace DiscordTaskBot.Infrastructure;

using Discord;
using Discord.WebSocket;
using DiscordTaskBot.Core;

/*
public class DiscordMessageService : IDiscordMessageService
{
    private readonly DiscordSocketClient _client;
    private readonly ulong _archiveChannelID;

    public DiscordMessageService(DiscordSocketClient client, ulong archiveChannelID)
    {
        _client = client;
        _archiveChannelID = archiveChannelID;
    }

    public async Task<TaskLocation> CreateTaskMessageAsync(ulong channelID, TaskItem taskItem)
    {
        var channel = await GetChannelByID(channelID);

        var message = await channel.SendMessageAsync(); //TODO

        return new TaskLocation(channelID, message.Id);
    }

    public async Task<bool> DeleteTaskMessageAsync(TaskLocation taskLocation)
    {
        var channel = await GetChannelByID(taskLocation.ChannelID);

        var message = await channel.GetMessageAsync(taskLocation.MessageID);

        if (message == null) return false;

        await message.DeleteAsync();

        return true;
    }

    public async Task<TaskLocation> MoveTaskMessageToArchiveAsync(TaskItem taskItem)
    {
        if (taskItem.TaskLocation == null) throw new InfrastructureException("Message location was not set");

        var archiveChannel = (ITextChannel)await _client.GetChannelAsync(_archiveChannelID) ?? throw new InfrastructureException("Archive channel could not be found");

        var channel = await GetChannelByID(taskItem.TaskLocation.ChannelID);

        var originalMessage = await channel.GetMessageAsync(taskItem.TaskLocation.MessageID) ?? throw new InfrastructureException("Message was not found");

        var newMessage = await archiveChannel.SendMessageAsync(
            text: originalMessage.Content,
            embeds: originalMessage.Embeds.ToArray(),
            components:
        )
    }

    private async Task<ITextChannel> GetChannelByID(ulong channelID)
    {
        return (ITextChannel)await _client.GetChannelAsync(channelID) ?? throw new InfrastructureException($"Channel of id: {channelID} could not be found");
    }
}

*/