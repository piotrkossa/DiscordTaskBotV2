namespace DiscordTaskBot.Infrastructure;

using Microsoft.EntityFrameworkCore;
using DiscordTaskBot.Domain;

public class TaskItemDbContext(DbContextOptions options) : DbContext(options)
{
    public DbSet<TaskItem> Tasks { get; set; }

    protected override void OnConfiguring(DbContextOptionsBuilder options)
    {
        options.EnableSensitiveDataLogging();
    }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<TaskItem>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Id)
                .ValueGeneratedNever();

            entity.Property(e => e.Description);

            entity.Property(e => e.AssigneeID);

            entity.Property(e => e.State)
                .HasConversion<string>();

            entity.OwnsOne(e => e.TaskDuration, duration =>
            {
                duration.Property(d => d.UtcCreationDate);

                duration.Property(d => d.UtcDueDate);
            });

            entity.HasIndex(e => e.AssigneeID);
            entity.HasIndex(e => e.State);
        });

        base.OnModelCreating(modelBuilder);
    }
}