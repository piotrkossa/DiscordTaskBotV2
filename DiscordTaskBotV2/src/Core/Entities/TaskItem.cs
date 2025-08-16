namespace DiscordTaskBot.Core;

public class TaskItem(string description, TaskDuration taskDuration, ulong asigneeID)
{
    public string ID = Guid.NewGuid().ToString();
    
    public string Description { get; private set; } = description;

    public TaskDuration TaskDuration { get; private set; } = taskDuration;

    public TaskState State { get; private set; } = TaskState.NOT_STARTED;

    public ulong AssigneeID { get; private set; } = asigneeID;

    public TaskLocation? TaskLocation { get; private set; }

    public void SetLocation(TaskLocation taskLocation)
    {
        TaskLocation = taskLocation;
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
    }
}
