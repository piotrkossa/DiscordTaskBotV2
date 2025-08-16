namespace DiscordTaskBot.Core;

public class TaskService(ITaskRepository taskRepository, IUserInformationService userInformationService, IDiscordService discordService)
{
    private readonly ITaskRepository _taskRepository = taskRepository;
    private readonly IUserInformationService _userInformationService = userInformationService;
    private readonly IDiscordService _discordService = discordService;

    public async Task CreateTaskAsync(string description, TaskDuration taskDuration, ulong requesterID, ulong assigneeID, ulong channelID)
    {
        await EnsureAdminAsync(requesterID);

        TaskItem taskItem = new(description, taskDuration, assigneeID);

        TaskLocation taskLocation = await _discordService.CreateTaskMessageAsync(channelID, taskItem);

        taskItem.SetLocation(taskLocation);

        await _taskRepository.AddTaskAsync(taskItem);
    }

    public async Task DeleteTaskAsync(string taskID, ulong requesterID)
    {
        await EnsureAdminAsync(requesterID);

        if (!await _taskRepository.DeleteTaskByIDAsync(taskID))
        {
            throw new DomainException("Task was not found");
        }
    }

    public async Task ChangeTaskState(string taskID, TaskState taskState, ulong requesterID)
    {
        TaskItem taskItem = await GetTaskAsync(taskID);

        if (requesterID != taskItem.AssigneeID && !await _userInformationService.IsAdminAsync(requesterID))
        {
            throw new DomainException("You can not edit tasks that are not yours");
        }

        taskItem.ChangeState(taskState);

        if (taskItem.State == TaskState.ARCHIVED)
        {
            taskItem.SetLocation(await _discordService.MoveTaskMessageToArchiveAsync(taskItem));
        }

        await _taskRepository.UpdateTaskAsync(taskItem);
    }

    public async Task RescheduleTask(string taskID, DateTime newDate, ulong requesterID)
    {
        await EnsureAdminAsync(requesterID);

        TaskItem taskItem = await GetTaskAsync(taskID);

        taskItem.TaskDuration.Reschedule(newDate);

        await _taskRepository.UpdateTaskAsync(taskItem);
    }

    public async Task RescheduleTask(string taskID, int daysToAdd, ulong requesterID)
    {
        await EnsureAdminAsync(requesterID);

        TaskItem taskItem = await GetTaskAsync(taskID);

        taskItem.TaskDuration.Reschedule(taskItem.TaskDuration.DueDate.AddDays(daysToAdd));

        await _taskRepository.UpdateTaskAsync(taskItem);
    }

    private async Task<TaskItem> GetTaskAsync(string taskID)
    {
        return await _taskRepository.GetTaskByIDAsync(taskID) ?? throw new DomainException("Task was not found");
    }

    private async Task EnsureAdminAsync(ulong userID)
    {
        if (!await _userInformationService.IsAdminAsync(userID))
        {
            throw new DomainException("Only administrators can delete tasks");
        }
    }
}