namespace DiscordTaskBot.Application;

using DiscordTaskBot.Domain;

using MediatR;

public class AddTimeCommandHandler : IRequestHandler<AddTimeCommand, TaskItem>
{
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
        
        task.TaskDuration.ExtendByDays(request.Days);
        
        await _taskRepository.UpdateAsync(task);

        return task;
    }
}