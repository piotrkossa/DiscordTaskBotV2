namespace DiscordTaskBot.Application;

using DiscordTaskBot.Domain;
using MediatR;

public class ChangeAssigneesCommandHandler : IRequestHandler<ChangeAssigneesCommand, ChangeAssigneesResult>
{
    private readonly ITaskRepository _taskRepository;
    private readonly IAuthorizationService _authorizationService;

    public ChangeAssigneesCommandHandler(ITaskRepository taskRepository, IAuthorizationService authorizationService)
    {
        _taskRepository = taskRepository;
        _authorizationService = authorizationService;
    }

    public async Task<ChangeAssigneesResult> Handle(ChangeAssigneesCommand request, CancellationToken cancellationToken)
    {
        if (!_authorizationService.CanEditTasksAsync(request.RequesterID))
            throw new DomainException("You do not have permission to edit tasks");

        var task = await _taskRepository.GetByIdAsync(request.TaskId) ?? throw new DomainException("Task not found");

        if (task.State == TaskState.ARCHIVED)
            throw new DomainException("Archived tasks cannot be changed");

        var added = task.ChangeAssignees(request.AssigneeIds);

        await _taskRepository.UpdateAsync(task);

        return new ChangeAssigneesResult(task, added);
    }
}
