namespace DiscordTaskBot.Infrastructure;

using Microsoft.EntityFrameworkCore;
using DiscordTaskBot.Domain;
using DiscordTaskBot.Application;
using System.Threading.Tasks;
using System.Collections.Generic;
using System;

public class TaskRepository : ITaskRepository
{
    private readonly TaskItemDbContext _dbContext;

    public TaskRepository(TaskItemDbContext context)
    {
        _dbContext = context;
        _dbContext.Database.EnsureCreated();
    }

    public async Task AddAsync(TaskItem taskItem)
    {
        if (taskItem == null)
        {
            throw new InfrastructureException("Invalid task item");
        }
       
        await _dbContext.Tasks.AddAsync(taskItem);
        await _dbContext.SaveChangesAsync();
    }

    public async Task<IEnumerable<TaskItem>> GetAllAsync()
    {
        return await _dbContext.Tasks.ToListAsync();
    }

    public async Task<TaskItem?> GetByIdAsync(Guid taskId)
    {
        var task = await _dbContext.Tasks.FindAsync(taskId);
        return task;
    }

    public async Task RemoveAsync(TaskItem taskItem)
    {
        var task = await _dbContext.Tasks.FindAsync(taskItem);
        if (task == null)
        {
            throw new InfrastructureException("Task item not found");
        }

        _dbContext.Tasks.Remove(task);
        await _dbContext.SaveChangesAsync();
    }

    public async Task UpdateAsync(TaskItem taskItem)
    {
        var existingTask = await _dbContext.Tasks.FindAsync(taskItem) ?? throw new InfrastructureException("Task not found for update");
        
        _dbContext.Entry(existingTask).CurrentValues.SetValues(taskItem);
        await _dbContext.SaveChangesAsync();
    }
}
