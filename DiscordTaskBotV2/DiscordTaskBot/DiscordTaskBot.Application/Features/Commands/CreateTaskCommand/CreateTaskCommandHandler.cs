namespace DiscordTaskBot.Application;

using DiscordTaskBot.Domain;

using MediatR;

public class CreateTaskCommandHandler : IRequestHandler<CreateTaskCommand, TaskItem>
{
    private readonly ITaskRepository _taskRepository;
    private readonly IAuthorizationService _authorizationService;
    

    public CreateTaskCommandHandler(ITaskRepository taskRepository, IAuthorizationService authorizationService)
    {
        _taskRepository = taskRepository;
        _authorizationService = authorizationService;
    }

    public async Task<TaskItem> Handle(CreateTaskCommand request, CancellationToken cancellationToken)
    {
        if (!_authorizationService.CanCreateTasksAsync(request.RequesterID))
            throw new DomainException("You do not have permission to create tasks");
        
        var task = new TaskItem(request.Description, request.TaskDuration, request.AsigneeId);
        await _taskRepository.AddAsync(task);
        return task;
    }
}