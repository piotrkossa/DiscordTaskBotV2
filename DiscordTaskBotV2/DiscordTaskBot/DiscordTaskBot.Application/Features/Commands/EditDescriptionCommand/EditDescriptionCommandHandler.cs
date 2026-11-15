namespace DiscordTaskBot.Application;

using DiscordTaskBot.Domain;
using MediatR;

public class EditDescriptionCommandHandler : IRequestHandler<EditDescriptionCommand, TaskItem>
{
    private readonly ITaskRepository _taskRepository;
    private readonly IAuthorizationService _authorizationService;

    public EditDescriptionCommandHandler(ITaskRepository taskRepository, IAuthorizationService authorizationService)
    {
        _taskRepository = taskRepository;
        _authorizationService = authorizationService;
    }

    public async Task<TaskItem> Handle(EditDescriptionCommand request, CancellationToken cancellationToken)
    {
        if (!_authorizationService.CanEditTasksAsync(request.RequesterID))
            throw new DomainException("You do not have permission to edit tasks");

        var task = await _taskRepository.GetByIdAsync(request.TaskId) ?? throw new DomainException("Task not found");

        task.ChangeDescription(request.Description);

        await _taskRepository.UpdateAsync(task);

        return task;
    }
}
