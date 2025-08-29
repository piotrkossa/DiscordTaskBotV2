namespace DiscordTaskBot.Application;

using DiscordTaskBot.Domain;

public interface ITaskRepository
{
    Task AddAsync(TaskItem taskItem);

    // throws InfrastructureException if task was not found
    Task RemoveAsync(TaskItem taskItem);

    // returns TaskItem if found, null if not found
    Task<TaskItem?> GetByIdAsync(Guid taskId);

    // throws InfrastructureException if task not found
    Task UpdateAsync(TaskItem taskItem);

    // returns all tasks
    Task<IEnumerable<TaskItem>> GetAllAsync();
}