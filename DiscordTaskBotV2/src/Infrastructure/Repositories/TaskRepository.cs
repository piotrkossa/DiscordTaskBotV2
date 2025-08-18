using Microsoft.EntityFrameworkCore;
using DiscordTaskBot.Core;

namespace DiscordTaskBot.Infrastructure;

public class TaskRepository : ITaskRepository
{
    private readonly TaskItemDbContext _dbContext;

    public TaskRepository(TaskItemDbContext context)
    {
        _dbContext = context;
        _dbContext.Database.EnsureCreated();
    }

    public async Task AddTaskAsync(TaskItem taskItem)
    {
        if (taskItem == null)
        {
            throw new InfrastructureException("Invalid task item");
        }

        await _dbContext.Tasks.AddAsync(taskItem);
        await _dbContext.SaveChangesAsync();
    }

    public async Task<bool> DeleteTaskByIDAsync(string taskID)
    {
        var task = await _dbContext.Tasks.FindAsync(taskID);
        if (task == null)
        {
            return false;
        }

        _dbContext.Tasks.Remove(task);
        return await _dbContext.SaveChangesAsync() > 0;
    }

    public async Task<TaskItem?> GetTaskByIDAsync(string taskId)
    {
        return await _dbContext.Tasks.FindAsync(taskId);
    }

    public async Task UpdateTaskAsync(TaskItem taskItem)
    {
        var existingTask = await _dbContext.Tasks.FindAsync(taskItem.ID);
        if (existingTask == null)
        {
            throw new InfrastructureException("Task not found for update");
        }

        _dbContext.Entry(existingTask).CurrentValues.SetValues(taskItem);
        await _dbContext.SaveChangesAsync();
    }

    public async Task<IEnumerable<TaskItem>> GetAllTasksAsync()
    {
        return await _dbContext.Tasks.ToListAsync();
    }
}