namespace DiscordTaskBot.Application;

public interface IAuthorizationService
{
    Task<bool> CanCreateTasksAsync(ulong userId);
    Task<bool> CanDeleteTasksAsync(ulong userId);
    Task<bool> CanEditTasksAsync(ulong userId);
}