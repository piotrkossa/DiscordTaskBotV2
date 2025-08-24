namespace DiscordTaskBot.Core;

public interface IUserInformationService
{
    // returns username, throws InfrastructureException if user not found
    Task<string> GetUsernameAsync(ulong userID);
    // returns true if admin, throws InfrastructureException if user not found
    Task<bool> IsAdminAsync(ulong userID);
}