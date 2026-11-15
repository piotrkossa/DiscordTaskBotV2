namespace DiscordTaskBot.Domain;

public enum ReminderKind
{
    // Sent once when the deadline is within TaskItem.ReminderWindow.
    DueSoon,
    // Sent once after the deadline has passed.
    Overdue
}
