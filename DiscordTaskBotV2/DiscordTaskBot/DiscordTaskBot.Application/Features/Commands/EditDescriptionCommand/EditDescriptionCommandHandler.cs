namespace DiscordTaskBot.Application;

using DiscordTaskBot.Domain;
using MediatR;

public class EditDescriptionCommandHandler : IRequestHandler<EditDescriptionCommand, TaskItem>
{
    private readonly ITaskRepository _taskRepository;
    private readonly IAuthorizationService _authorizationService;
    private readonly TaskLocks _locks;

    public EditDescriptionCommandHandler(ITaskRepository taskRepository, IAuthorizationService authorizationService, TaskLocks locks)
    {
        _taskRepository = taskRepository;
        _authorizationService = authorizationService;
        _locks = locks;
    }

    public async Task<TaskItem> Handle(EditDescriptionCommand request, CancellationToken cancellationToken)
    {
        if (!_authorizationService.CanEditTasksAsync(request.RequesterID))
            throw new DomainException("You do not have permission to edit tasks");

        using var _ = await _locks.AcquireAsync(request.TaskId);

        var task = await _taskRepository.GetByIdAsync(request.TaskId) ?? throw new DomainException("Task not found");

        task.ChangeDescription(request.Description);

        await _taskRepository.UpdateAsync(task);

        return task;
    }
}
