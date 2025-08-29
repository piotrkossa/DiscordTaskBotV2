namespace DiscordTaskBot.Core;

public class TaskItem
{
    public string ID { get; }
    
    public string Description { get; private set; }

    public TaskDuration TaskDuration { get; private set; }

    public TaskState State { get; private set; }

    public ulong AssigneeID { get; private set; }

    public TaskLocation? TaskLocation { get; private set; }

    public TaskItem(string description, TaskDuration taskDuration, ulong asigneeID)
    {
        ID = Guid.NewGuid().ToString();
        Description = description;
        TaskDuration = taskDuration;
        State = TaskState.NOT_STARTED;
        AssigneeID = asigneeID;
    }

    private TaskItem() {}


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
