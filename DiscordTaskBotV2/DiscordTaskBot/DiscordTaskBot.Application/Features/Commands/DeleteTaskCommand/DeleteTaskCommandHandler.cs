namespace DiscordTaskBot.Application;

using DiscordTaskBot.Domain;
using MediatR;

public class DeleteTaskCommandHandler : IRequestHandler<DeleteTaskCommand, TaskItem>
{
    private readonly ITaskRepository _taskRepository;
    private readonly IAuthorizationService _authorizationService;

    public DeleteTaskCommandHandler(ITaskRepository taskRepository, IAuthorizationService authorizationService)
    {
        _taskRepository = taskRepository;
        _authorizationService = authorizationService;
    }

    public async Task<TaskItem> Handle(DeleteTaskCommand request, CancellationToken cancellationToken)
    {
        if (!await _authorizationService.CanDeleteTasksAsync(request.RequesterID))
            throw new DomainException("You do not have permission to delete tasks");

        var taskItem = await _taskRepository.GetByIdAsync(request.TaskId) ?? throw new DomainException("Task could not be found");

        await _taskRepository.RemoveAsync(taskItem);

        return taskItem;
    }
}