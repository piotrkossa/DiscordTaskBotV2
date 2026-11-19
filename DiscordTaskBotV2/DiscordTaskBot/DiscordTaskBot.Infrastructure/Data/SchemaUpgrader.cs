namespace DiscordTaskBot.Infrastructure;

using System.Data.Common;
using Microsoft.EntityFrameworkCore;

/// <summary>
/// Brings a database created by an older version of the bot up to the current schema without losing data.
/// EnsureCreated() only creates a missing database - it never changes an existing one - so columns added
/// in newer versions are added here. Before any change a copy of the database is saved next to it.
/// </summary>
public static class SchemaUpgrader
{
    private const string Table = "Tasks";

    // Columns added after the first released schema: name, SQL definition.
    private static readonly (string Name, string Definition)[] AddedColumns =
    [
        ("AssigneeIDs", "TEXT NOT NULL DEFAULT ''"),
        ("ChannelId", "INTEGER NOT NULL DEFAULT 0"),
        ("MessageId", "INTEGER NOT NULL DEFAULT 0"),
        ("DueSoonReminderSent", "INTEGER NOT NULL DEFAULT 0"),
        ("OverdueReminderSent", "INTEGER NOT NULL DEFAULT 0"),
        ("StartedAssigneeIDs", "TEXT NOT NULL DEFAULT ''"),
        ("CompletedAssigneeIDs", "TEXT NOT NULL DEFAULT ''"),
    ];

    public static void Upgrade(TaskItemDbContext dbContext, Action<string>? log = null)
    {
        var database = dbContext.Database;
        database.OpenConnection();
        try
        {
            var connection = database.GetDbConnection();
            var columns = GetColumns(connection);

            var missing = AddedColumns.Where(c => !columns.Contains(c.Name)).ToList();
            var hasLegacyAssignee = columns.Contains("AssigneeID");

            if (missing.Count == 0 && !hasLegacyAssignee)
                return;

            var backupPath = Backup(connection);
            log?.Invoke($"Upgrading database schema (backup saved to {backupPath})");

            using var transaction = database.BeginTransaction();

            foreach (var (name, definition) in missing)
            {
                database.ExecuteSqlRaw($"ALTER TABLE \"{Table}\" ADD COLUMN \"{name}\" {definition}");
                log?.Invoke($"  added column {name}");
            }

            // The very first version had a single assignee column.
            if (hasLegacyAssignee)
            {
                database.ExecuteSqlRaw($"UPDATE \"{Table}\" SET \"AssigneeIDs\" = CAST(\"AssigneeID\" AS TEXT) WHERE \"AssigneeIDs\" = ''");
                database.ExecuteSqlRaw("DROP INDEX IF EXISTS \"IX_Tasks_AssigneeID\"");
                database.ExecuteSqlRaw($"ALTER TABLE \"{Table}\" DROP COLUMN \"AssigneeID\"");
                log?.Invoke("  migrated AssigneeID to AssigneeIDs");
            }

            // Tasks from before per-person progress: everyone is treated as having reached the task's state.
            if (missing.Any(c => c.Name == "StartedAssigneeIDs"))
                database.ExecuteSqlRaw($"UPDATE \"{Table}\" SET \"StartedAssigneeIDs\" = \"AssigneeIDs\" WHERE \"State\" IN ('IN_PROGRESS', 'COMPLETED', 'ARCHIVED')");
            if (missing.Any(c => c.Name == "CompletedAssigneeIDs"))
                database.ExecuteSqlRaw($"UPDATE \"{Table}\" SET \"CompletedAssigneeIDs\" = \"AssigneeIDs\" WHERE \"State\" IN ('COMPLETED', 'ARCHIVED')");

            transaction.Commit();
            log?.Invoke("Database schema upgraded");
        }
        finally
        {
            database.CloseConnection();
        }
    }

    private static HashSet<string> GetColumns(DbConnection connection)
    {
        var columns = new HashSet<string>(StringComparer.OrdinalIgnoreCase);

        using var command = connection.CreateCommand();
        command.CommandText = $"PRAGMA table_info(\"{Table}\")";
        using var reader = command.ExecuteReader();
        while (reader.Read())
            columns.Add(reader.GetString(1));

        return columns;
    }

    // VACUUM INTO writes a consistent copy even while the database is open.
    private static string Backup(DbConnection connection)
    {
        var source = string.IsNullOrEmpty(connection.DataSource) ? "taskitems.db" : connection.DataSource;
        var backupPath = Path.GetFullPath($"{source}.backup-{DateTime.Now:yyyyMMdd-HHmmss}");

        using var command = connection.CreateCommand();
        command.CommandText = $"VACUUM INTO '{backupPath.Replace("'", "''")}'";
        command.ExecuteNonQuery();

        return backupPath;
    }
}
