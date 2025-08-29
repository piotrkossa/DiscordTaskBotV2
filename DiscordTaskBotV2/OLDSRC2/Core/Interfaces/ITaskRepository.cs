namespace DiscordTaskBot.Core;

public interface ITaskRepository
{
    Task AddTaskAsync(TaskItem taskItem);
    // returns true if task was found and deleted, false if task was not found
    Task<bool> DeleteTaskByIDAsync(string taskID);

    // returns TaskItem if found, null if not found
    Task<TaskItem?> GetTaskByIDAsync(string taskId);

    // returns true if task updated successfully, false if not found
    Task<bool> UpdateTaskAsync(TaskItem taskItem);
    
    // returns all tasks
    Task<IEnumerable<TaskItem>> GetAllTasksAsync();
}