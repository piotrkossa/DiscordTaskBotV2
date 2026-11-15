namespace DiscordTaskBot.Infrastructure;

using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;
using DiscordTaskBot.Domain;

public class TaskItemDbContext(DbContextOptions<TaskItemDbContext> options) : DbContext(options)
{
    public DbSet<TaskItem> Tasks { get; set; }

    // SQLite does not store DateTimeKind, so mark every value read back as UTC.
    private static readonly ValueConverter<DateTime, DateTime> UtcConverter = new(
        v => v.Kind == DateTimeKind.Local ? v.ToUniversalTime() : v,
        v => DateTime.SpecifyKind(v, DateTimeKind.Utc));

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
                duration.Property(d => d.UtcCreationDate)
                    .HasConversion(UtcConverter);

                duration.Property(d => d.UtcDueDate)
                    .HasConversion(UtcConverter);
            });

            entity.HasIndex(e => e.AssigneeID);
            entity.HasIndex(e => e.State);
        });

        base.OnModelCreating(modelBuilder);
    }
}
