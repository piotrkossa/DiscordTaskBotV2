namespace DiscordTaskBot.Core;

public interface ITaskRepository
{
    Task AddTaskAsync(TaskItem taskItem);
    // returns true if task was found and deleted, false if task was not found
    Task<bool> DeleteTaskByIDAsync(string taskID);

    Task<TaskItem?> GetTaskByIDAsync(string taskId);
    
    Task UpdateTaskAsync(TaskItem taskItem);
}