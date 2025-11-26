namespace DiscordTaskBot.Domain;

public class TaskItem : Entity
{
    public string Description { get; private set; }

    public TaskDuration TaskDuration { get; private set; }

    public TaskState State { get; private set; }

    public ulong AssigneeID { get; private set; }

    private TaskItem() : base(default) {}

    public TaskItem(string description, TaskDuration taskDuration, ulong asigneeID) : base(Guid.NewGuid())
    {
        Description = description;
        TaskDuration = taskDuration;
        State = TaskState.NOT_STARTED;
        AssigneeID = asigneeID;

        _domainEvents.Add(new TaskCreatedEvent(this));
    }

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
}
