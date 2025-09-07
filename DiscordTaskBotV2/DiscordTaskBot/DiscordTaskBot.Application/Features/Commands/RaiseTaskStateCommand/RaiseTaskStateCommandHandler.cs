namespace DiscordTaskBot.Application;

using DiscordTaskBot.Domain;
using MediatR;

public class RaiseTaskStateCommandHandler : IRequestHandler<RaiseTaskStateCommand, TaskItem>
{
    private readonly ITaskRepository _taskRepository;
    private readonly IAuthorizationService _authorizationService;

    public RaiseTaskStateCommandHandler(ITaskRepository taskRepository, IAuthorizationService authorizationService)
    {
        _taskRepository = taskRepository;
        _authorizationService = authorizationService;
    }

    public async Task<TaskItem> Handle(RaiseTaskStateCommand request, CancellationToken cancellationToken)
    {
        var taskItem = await _taskRepository.GetByIdAsync(request.TaskId) ?? throw new DomainException("Task could not be found");

        if (!_authorizationService.CanEditTasksAsync(request.RequesterID) && taskItem.AssigneeID != request.RequesterID)
            throw new DomainException("You do not have permission to update this task");

        taskItem.RaiseState();

        await _taskRepository.UpdateAsync(taskItem);

        return taskItem;
    }
}