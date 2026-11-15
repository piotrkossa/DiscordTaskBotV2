namespace DiscordTaskBot.Application;

using DiscordTaskBot.Domain;

using MediatR;

public class AddTimeCommandHandler : IRequestHandler<AddTimeCommand, TaskItem>
{
    // Days can be negative to move the deadline back. One year either way is plenty.
    public const int MaxDaysChange = 365;

    private readonly ITaskRepository _taskRepository;
    private readonly IAuthorizationService _authorizationService;

    public AddTimeCommandHandler(ITaskRepository taskRepository, IAuthorizationService authorizationService)
    {
        _taskRepository = taskRepository;
        _authorizationService = authorizationService;
    }

    public async Task<TaskItem> Handle(AddTimeCommand request, CancellationToken cancellationToken)
    {
        if (!_authorizationService.CanCreateTasksAsync(request.RequesterID))
            throw new DomainException("You do not have permission to add time to tasks");
        
        var task = await _taskRepository.GetByIdAsync(request.TaskId);
        
        if (task == null)
            throw new DomainException("Task not found");
        
        if (request.Days == 0 || Math.Abs(request.Days) > MaxDaysChange)
            throw new DomainException($"Days must be between -{MaxDaysChange} and {MaxDaysChange} and cannot be 0.");

        // Moving the deadline back must not put it at or before the moment the task was created.
        var newDueDate = task.TaskDuration.UtcDueDate.AddDays(request.Days);
        if (newDueDate <= task.TaskDuration.UtcCreationDate)
        {
            var maxBack = (int)Math.Ceiling((task.TaskDuration.UtcDueDate - task.TaskDuration.UtcCreationDate).TotalDays) - 1;
            throw new DomainException(maxBack > 0
                ? $"The deadline cannot be moved before the task was created. You can move it back by at most {maxBack} days."
                : "The deadline cannot be moved back any further - it is already less than a day after the task was created.");
        }

        task.MoveDeadline(request.Days, DateTime.UtcNow);
        
        await _taskRepository.UpdateAsync(task);

        return task;
    }
}