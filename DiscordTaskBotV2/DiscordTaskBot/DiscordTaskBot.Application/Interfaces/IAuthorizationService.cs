namespace DiscordTaskBot.Application;

public interface IAuthorizationService
{
    bool CanCreateTasksAsync(ulong userId);
    bool CanDeleteTasksAsync(ulong userId);
    bool CanEditTasksAsync(ulong userId);
}