namespace DiscordTaskBot.Application;

using DiscordTaskBot.Domain;
using MediatR;

public class UpdateTaskStateCommandHandler : IRequestHandler<UpdateTaskStateCommand, TaskItem>
{
    private readonly ITaskRepository _taskRepository;
    private readonly IAuthorizationService _authorizationService;

    public UpdateTaskStateCommandHandler(ITaskRepository taskRepository, IAuthorizationService authorizationService)
    {
        _taskRepository = taskRepository;
        _authorizationService = authorizationService;
    }

    public async Task<TaskItem> Handle(UpdateTaskStateCommand request, CancellationToken cancellationToken)
    {
        var taskItem = await _taskRepository.GetByIdAsync(request.TaskId) ?? throw new DomainException("Task could not be found");

        if (!await _authorizationService.CanEditTasksAsync(request.RequesterID) && taskItem.AssigneeID != request.RequesterID)
            throw new DomainException("You do not have permission to update this task");

        taskItem.ChangeState(request.TaskState);

        await _taskRepository.UpdateAsync(taskItem);

        return taskItem;
    }
}