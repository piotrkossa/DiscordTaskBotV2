using Discord;
using Discord.WebSocket;
using DiscordTaskBot.Core;

namespace DiscordTaskBot.Infrastructure;

public class UserInformationService : IUserInformationService
{
    private readonly DiscordSocketClient _client;

    public UserInformationService(DiscordSocketClient client)
    {
        _client = client;
    }

    public async Task<string> GetUsernameAsync(ulong userID)
    {
        var user = await GetUser(userID);

        return user.Username;
    }

    public async Task<bool> IsAdminAsync(ulong userID)
    {
        var user = (SocketGuildUser)await GetUser(userID);
        return user.GuildPermissions.Administrator;
    }

    private async Task<IUser> GetUser(ulong userID)
    {
        var user = await _client.GetUserAsync(userID) ?? throw new InfrastructureException("User could not be found");
        return user;
    }
}