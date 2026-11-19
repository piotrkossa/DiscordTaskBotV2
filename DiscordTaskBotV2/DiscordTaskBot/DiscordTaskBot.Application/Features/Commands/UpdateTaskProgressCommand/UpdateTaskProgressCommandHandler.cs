namespace DiscordTaskBot.Application;

using DiscordTaskBot.Domain;
using MediatR;

public class UpdateTaskProgressCommandHandler : IRequestHandler<UpdateTaskProgressCommand, TaskItem>
{
    private readonly ITaskRepository _taskRepository;
    private readonly IAuthorizationService _authorizationService;
    private readonly TaskLocks _locks;

    public UpdateTaskProgressCommandHandler(ITaskRepository taskRepository, IAuthorizationService authorizationService, TaskLocks locks)
    {
        _taskRepository = taskRepository;
        _authorizationService = authorizationService;
        _locks = locks;
    }

    public async Task<TaskItem> Handle(UpdateTaskProgressCommand request, CancellationToken cancellationToken)
    {
        // Several assignees can click at the same moment - without the lock one click could overwrite another.
        using var _ = await _locks.AcquireAsync(request.TaskId);

        var taskItem = await _taskRepository.GetByIdAsync(request.TaskId) ?? throw new DomainException("Task could not be found");

        switch (request.Action)
        {
            case TaskProgressAction.Start:
                taskItem.StartWork(request.RequesterID);
                break;

            case TaskProgressAction.Complete:
                taskItem.CompleteWork(request.RequesterID);
                break;

            case TaskProgressAction.Archive:
                // Check the assignee first so the assignee never depends on the guild-member lookup.
                if (!taskItem.IsAssignee(request.RequesterID) && !_authorizationService.CanEditTasksAsync(request.RequesterID))
                    throw new DomainException("You do not have permission to update this task");
                taskItem.Archive();
                break;

            default:
                throw new DomainException($"Unknown action {request.Action}");
        }

        await _taskRepository.UpdateAsync(taskItem);

        return taskItem;
    }
}
