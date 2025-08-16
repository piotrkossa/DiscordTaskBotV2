namespace DiscordTaskBot.Core;

public interface IUserInformationService
{
    Task<string> GetUsernameAsync(ulong userID);
    Task<bool> IsAdminAsync(ulong userID);
}