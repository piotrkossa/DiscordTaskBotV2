using Microsoft.EntityFrameworkCore;
using DiscordTaskBot.Core;

/*

namespace DiscordTaskBot.Infrastructure;

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
            entity.HasKey(e => e.ID);
            entity.Property(e => e.ID)
                .ValueGeneratedNever();

            entity.Property(e => e.Description);

            entity.Property(e => e.AssigneeID);

            entity.Property(e => e.State)
                .HasConversion<string>();

            entity.OwnsOne(e => e.TaskLocation, location =>
            {
                location.Property(l => l.ChannelID);

                location.Property(l => l.MessageID);
            });

            entity.OwnsOne(e => e.TaskDuration, duration =>
            {
                duration.Property(d => d.CreationDate);

                duration.Property(d => d.DueDate);

                duration.Property(d => d.TimeZone).HasConversion(
                    tz => tz.Id,
                    id => TimeZoneInfo.FindSystemTimeZoneById(id)
                );
            });

            entity.HasIndex(e => e.AssigneeID);
            entity.HasIndex(e => e.State);
        });

        base.OnModelCreating(modelBuilder);
    }
}

*/