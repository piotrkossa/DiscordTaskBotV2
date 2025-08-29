namespace DiscordTaskBot.Infrastructure;

using System.Threading.Tasks;
using DiscordTaskBot.Application;
using Discord.WebSocket;

public class AuthorizationService : IAuthorizationService
{
    public DiscordSocketClient _client;

    public AuthorizationService(DiscordSocketClient client)
    {
        _client = client;
    }


    public async Task<bool> CanCreateTasksAsync(ulong userId)
    {
        return await IsAdmin(userId);
    }

    public async Task<bool> CanDeleteTasksAsync(ulong userId)
    {
        return await IsAdmin(userId);
    }

    public async Task<bool> CanEditTasksAsync(ulong userId)
    {
        return await IsAdmin(userId);
    }

    private async Task<bool> IsAdmin(ulong userId)
    {
        SocketGuildUser user = (SocketGuildUser)await _client.GetUserAsync(userId);
        if (user.GuildPermissions.Administrator)
        {
            return true;
        }
        return false;
    }
}