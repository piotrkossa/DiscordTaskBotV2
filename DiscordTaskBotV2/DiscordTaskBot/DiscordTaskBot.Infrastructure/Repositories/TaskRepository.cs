namespace DiscordTaskBot.Infrastructure;

using Microsoft.EntityFrameworkCore;
using DiscordTaskBot.Domain;
using DiscordTaskBot.Application;
using System.Threading.Tasks;
using System.Collections.Generic;
using System;

// The repository is a singleton shared by concurrently running interactions,
// so every operation uses its own short-lived DbContext (DbContext is not thread-safe
// and a long-lived one would also keep serving stale, cached entities).
public class TaskRepository : ITaskRepository
{
    private readonly IDbContextFactory<TaskItemDbContext> _contextFactory;

    public TaskRepository(IDbContextFactory<TaskItemDbContext> contextFactory)
    {
        _contextFactory = contextFactory;

        using var dbContext = _contextFactory.CreateDbContext();
        dbContext.Database.EnsureCreated();

        // Adds columns introduced by newer versions to an existing database, keeping all data.
        SchemaUpgrader.Upgrade(dbContext, message => Console.WriteLine($"[DATABASE] {message}"));
    }

    public async Task AddAsync(TaskItem taskItem)
    {
        if (taskItem == null)
        {
            throw new InfrastructureException("Invalid task item");
        }

        await using var dbContext = await _contextFactory.CreateDbContextAsync();
        await dbContext.Tasks.AddAsync(taskItem);
        await dbContext.SaveChangesAsync();
    }

    public async Task<IEnumerable<TaskItem>> GetAllAsync()
    {
        await using var dbContext = await _contextFactory.CreateDbContextAsync();
        return await dbContext.Tasks.AsNoTracking().ToListAsync();
    }

    public async Task<TaskItem?> GetByIdAsync(Guid taskId)
    {
        await using var dbContext = await _contextFactory.CreateDbContextAsync();
        return await dbContext.Tasks.AsNoTracking().FirstOrDefaultAsync(t => t.Id == taskId);
    }

    public async Task RemoveAsync(TaskItem taskItem)
    {
        await using var dbContext = await _contextFactory.CreateDbContextAsync();

        var task = await dbContext.Tasks.FindAsync(taskItem.Id)
            ?? throw new InfrastructureException("Task item not found");

        dbContext.Tasks.Remove(task);
        await dbContext.SaveChangesAsync();
    }

    public async Task SetMessageLocationAsync(Guid taskId, ulong channelId, ulong messageId)
    {
        await using var dbContext = await _contextFactory.CreateDbContextAsync();
        await dbContext.Tasks
            .Where(t => t.Id == taskId)
            .ExecuteUpdateAsync(s => s
                .SetProperty(t => t.ChannelId, channelId)
                .SetProperty(t => t.MessageId, messageId));
    }

    public async Task MarkReminderSentAsync(Guid taskId, ReminderKind kind)
    {
        await using var dbContext = await _contextFactory.CreateDbContextAsync();
        var query = dbContext.Tasks.Where(t => t.Id == taskId);

        // Same rules as TaskItem.MarkReminderSent.
        if (kind == ReminderKind.Overdue)
            await query.ExecuteUpdateAsync(s => s
                .SetProperty(t => t.DueSoonReminderSent, true)
                .SetProperty(t => t.OverdueReminderSent, true));
        else
            await query.ExecuteUpdateAsync(s => s.SetProperty(t => t.DueSoonReminderSent, true));
    }

    public async Task UpdateAsync(TaskItem taskItem)
    {
        await using var dbContext = await _contextFactory.CreateDbContextAsync();

        if (!await dbContext.Tasks.AnyAsync(t => t.Id == taskItem.Id))
            throw new InfrastructureException("Task not found for update");

        // Update() marks the entity and its owned TaskDuration as modified,
        // so changes such as an extended due date are persisted too.
        dbContext.Tasks.Update(taskItem);
        await dbContext.SaveChangesAsync();
    }
}
