using Discord;
using Discord.Interactions;
using Discord.WebSocket;

namespace DiscordTaskBot.Services
{
    public class DiscordService
    {
        private DiscordSocketClient _client;

        public DiscordService(DiscordSocketClient client)
        {
            _client = client;
        }

        public async Task<IUserMessage> SendMessageAsync(Embed embed, MessageComponent? component, SocketInteractionContext context)
        {
            await context.Interaction.FollowupAsync(embed: embed, components: component);

            return await context.Interaction.GetOriginalResponseAsync();
        }

        public async Task UpdateMessageAsync(Embed embed, MessageComponent? component, IUserMessage message)
        {
            await message.ModifyAsync(msg =>
            {
                msg.Embed = embed;
                msg.Components = component;
            });
        }

        public async Task<IUserMessage?> MoveMessageAsync(SocketMessageComponent messageComponent)
        {
            if (await messageComponent.Channel.GetMessageAsync(messageComponent.Message.Id) is not IUserMessage originalMessage)
                return null;

            var embed = originalMessage.Embeds.FirstOrDefault() as Embed;
            var components = originalMessage.Components as MessageComponent;

            await originalMessage.DeleteAsync();

            var archiveChannelID = ulong.Parse(Environment.GetEnvironmentVariable("ARCHIVE_CHANNEL")!);

            var archiveChannel = _client.GetChannel(archiveChannelID) as IMessageChannel;

            if (archiveChannel == null)
                return null;

            var newMessage = await archiveChannel.SendMessageAsync(embed: embed, components: components);
            return newMessage;
        }

        public async Task<IUserMessage?> GetMessageAsync(ulong messageID, ulong channelID)
        {
            var channel = await _client.GetChannelAsync(channelID) as IMessageChannel;
            if (channel == null)
                return null;

            var message = await channel.GetMessageAsync(messageID) as IUserMessage;
            return message;
        }
    }
}