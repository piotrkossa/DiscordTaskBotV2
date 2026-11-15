namespace DiscordTaskBot.Domain;

public class TaskItem : Entity
{
    public const int MaxDescriptionLength = 1000;
    public const int MaxAssignees = 5;

    // Reminders start once the deadline is this close (or already passed).
    public static readonly TimeSpan ReminderWindow = TimeSpan.FromDays(2);

    public string Description { get; private set; } = string.Empty;

    public TaskDuration TaskDuration { get; private set; } = null!;

    public TaskState State { get; private set; }

    public IReadOnlyList<ulong> AssigneeIDs { get; private set; } = [];

    // Where the task message lives - 0 when unknown. Used for links in DMs.
    public ulong ChannelId { get; private set; }
    public ulong MessageId { get; private set; }

    // Each reminder is sent at most once.
    public bool DueSoonReminderSent { get; private set; }
    public bool OverdueReminderSent { get; private set; }

    private TaskItem() : base(default) {}

    public TaskItem(string description, TaskDuration taskDuration, IEnumerable<ulong> assigneeIDs) : base(Guid.NewGuid())
    {
        Description = ValidateDescription(description);
        TaskDuration = taskDuration;
        State = TaskState.NOT_STARTED;
        AssigneeIDs = ValidateAssignees(assigneeIDs);

        // A task created with 2 days or less already gets the deadline in its "new task" message,
        // so no separate "due soon" reminder right after it.
        DueSoonReminderSent = taskDuration.UtcDueDate - taskDuration.UtcCreationDate <= ReminderWindow;

        _domainEvents.Add(new TaskCreatedEvent(this));
    }

    public bool IsAssignee(ulong userId) => AssigneeIDs.Contains(userId);

    public void ChangeState(TaskState newState)
    {
        var isValidTransition = (State, newState) switch
        {
            (TaskState.NOT_STARTED, TaskState.IN_PROGRESS) => true,
            (TaskState.IN_PROGRESS, TaskState.COMPLETED) => true,
            (TaskState.COMPLETED, TaskState.ARCHIVED) => true,
            _ => false
        };

        if (!isValidTransition)
            throw new DomainException($"Cannot change state from {State} to {newState}");

        State = newState;
        _domainEvents.Add(new TaskStateUpdatedEvent(Id, State));
    }

    public void RaiseState()
    {
        ChangeState(State + 1);
    }

    public void ChangeDescription(string description)
    {
        Description = ValidateDescription(description);
    }

    /// <returns>The users that were not assigned before.</returns>
    public IReadOnlyList<ulong> ChangeAssignees(IEnumerable<ulong> assigneeIDs)
    {
        var newAssignees = ValidateAssignees(assigneeIDs);
        var added = newAssignees.Except(AssigneeIDs).ToArray();

        AssigneeIDs = newAssignees;
        return added;
    }

    public void AttachMessage(ulong channelId, ulong messageId)
    {
        ChannelId = channelId;
        MessageId = messageId;
    }

    /// <summary>The reminder that should be sent now, or null. Finished tasks get no reminders.</summary>
    public ReminderKind? GetDueReminder(DateTime utcNow)
    {
        if (State >= TaskState.COMPLETED)
            return null;

        if (!OverdueReminderSent && utcNow > TaskDuration.UtcDueDate)
            return ReminderKind.Overdue;

        if (!DueSoonReminderSent && TaskDuration.UtcDueDate - utcNow <= ReminderWindow)
            return ReminderKind.DueSoon;

        return null;
    }

    public void MarkReminderSent(ReminderKind kind)
    {
        // After the overdue reminder a "due soon" one makes no sense anymore.
        DueSoonReminderSent = true;
        if (kind == ReminderKind.Overdue)
            OverdueReminderSent = true;
    }

    /// <summary>
    /// Moves the deadline and re-arms the reminders: if the new deadline is far enough again, both reminders
    /// will be sent again in time; if it lands inside the reminder window (or in the past), the
    /// "deadline changed" message already tells the assignees, so that reminder is not repeated.
    /// </summary>
    public void MoveDeadline(int days, DateTime utcNow)
    {
        TaskDuration.ExtendByDays(days);

        var timeLeft = TaskDuration.UtcDueDate - utcNow;
        DueSoonReminderSent = timeLeft <= ReminderWindow;
        OverdueReminderSent = timeLeft <= TimeSpan.Zero;
    }

    private static string ValidateDescription(string description)
    {
        var trimmed = description?.Trim() ?? string.Empty;

        if (trimmed.Length == 0)
            throw new DomainException("Description cannot be empty.");

        if (trimmed.Length > MaxDescriptionLength)
            throw new DomainException($"Description cannot be longer than {MaxDescriptionLength} characters.");

        return trimmed;
    }

    private static ulong[] ValidateAssignees(IEnumerable<ulong> assigneeIDs)
    {
        var ids = (assigneeIDs ?? []).Where(id => id != 0).Distinct().ToArray();

        if (ids.Length == 0)
            throw new DomainException("A task needs at least one assignee.");

        if (ids.Length > MaxAssignees)
            throw new DomainException($"A task can have at most {MaxAssignees} assignees.");

        return ids;
    }
}
