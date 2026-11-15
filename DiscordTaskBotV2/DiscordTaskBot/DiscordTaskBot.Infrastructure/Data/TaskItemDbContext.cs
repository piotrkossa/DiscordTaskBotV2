namespace DiscordTaskBot.Infrastructure;

using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.ChangeTracking;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;
using DiscordTaskBot.Domain;

public class TaskItemDbContext(DbContextOptions<TaskItemDbContext> options) : DbContext(options)
{
    public DbSet<TaskItem> Tasks { get; set; }

    // SQLite does not store DateTimeKind, so mark every value read back as UTC.
    private static readonly ValueConverter<DateTime, DateTime> UtcConverter = new(
        v => v.Kind == DateTimeKind.Local ? v.ToUniversalTime() : v,
        v => DateTime.SpecifyKind(v, DateTimeKind.Utc));

    // Assignees are stored as "id1,id2,id3" in a single column.
    private static readonly ValueConverter<IReadOnlyList<ulong>, string> AssigneesConverter = new(
        v => string.Join(',', v),
        v => v.Split(',', StringSplitOptions.RemoveEmptyEntries).Select(ulong.Parse).ToArray());

    private static readonly ValueComparer<IReadOnlyList<ulong>> AssigneesComparer = new(
        (a, b) => a!.SequenceEqual(b!),
        v => v.Aggregate(0, (hash, id) => HashCode.Combine(hash, id.GetHashCode())),
        v => v.ToArray());

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<TaskItem>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Id)
                .ValueGeneratedNever();

            entity.Property(e => e.Description);

            entity.Property(e => e.AssigneeIDs)
                .HasConversion(AssigneesConverter, AssigneesComparer);

            entity.Property(e => e.ChannelId);
            entity.Property(e => e.MessageId);

            entity.Property(e => e.DueSoonReminderSent);
            entity.Property(e => e.OverdueReminderSent);

            entity.Property(e => e.State)
                .HasConversion<string>();

            entity.OwnsOne(e => e.TaskDuration, duration =>
            {
                duration.Property(d => d.UtcCreationDate)
                    .HasConversion(UtcConverter);

                duration.Property(d => d.UtcDueDate)
                    .HasConversion(UtcConverter);
            });

            entity.HasIndex(e => e.State);
        });

        base.OnModelCreating(modelBuilder);
    }
}
