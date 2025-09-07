namespace DiscordTaskBot.Infrastructure;

using DiscordTaskBot.Application;
using Discord.WebSocket;
using DiscordTaskBot.Presentation;
using Microsoft.Extensions.Options;

public class AuthorizationService : IAuthorizationService
{
    private readonly DiscordSocketClient _client;
    private readonly DiscordBotOptions _botOptions;

    public AuthorizationService(DiscordSocketClient client, IOptions<DiscordBotOptions> options)
    {
        _client = client;
        _botOptions = options.Value;
    }


    public bool CanCreateTasksAsync(ulong userId)
    {
        return IsAdmin(userId);
    }

    public bool CanDeleteTasksAsync(ulong userId)
    {
        return IsAdmin(userId);
    }

    public bool CanEditTasksAsync(ulong userId)
    {
        return IsAdmin(userId);
    }

    private bool IsAdmin(ulong userId)
    {
        var guildId = _botOptions.GuildId;

        var guild = _client.GetGuild(guildId) ?? throw new InfrastructureException($"Guild with Id: {guildId} was not found!");

        var user = guild.GetUser(userId) ?? throw new InfrastructureException($"User with Id: {userId} was not found in guild with Id: {guildId}");

        return user?.GuildPermissions.Administrator ?? false;
    }
}