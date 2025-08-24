namespace DiscordTaskBot.Core;

public class TaskService(
    ITaskRepository taskRepository,
    IUserInformationService userInformationService,
    IDiscordMessageManager discordMessageManager,
    IJobScheduler jobScheduler,
    DiscordMessageDirector messageDirector,
    BehaviorConfig behaviorConfig
    )
{
    private readonly ITaskRepository _taskRepository = taskRepository;
    private readonly IUserInformationService _userInformationService = userInformationService;
    private readonly IDiscordMessageManager _messageManager = discordMessageManager;
    private readonly IJobScheduler _jobScheduler = jobScheduler;

    private readonly DiscordMessageDirector _messageDirector = messageDirector;

    private readonly BehaviorConfig _behaviorConfig = behaviorConfig;

    public async Task CreateTaskAsync(string description, TaskDuration taskDuration, ulong requesterID, ulong assigneeID, ulong channelID)
    {
        await EnsureAdminAsync(requesterID);

        TaskItem taskItem = new(description, taskDuration, assigneeID);

        var message = _messageDirector.BuildTaskMessage(taskItem, GetTaskStatus(taskItem));
        var taskLocation = await message.SendToChannel(channelID);

        taskItem.SetLocation(taskLocation);

        await _taskRepository.AddTaskAsync(taskItem);

        ScheduleTaskJobs(taskItem);
    }

    public async Task DeleteTaskAsync(string taskID, ulong requesterID)
    {
        await EnsureAdminAsync(requesterID);

        TaskItem taskItem = await GetTaskAsync(taskID);

        TaskLocation taskLocation = taskItem.TaskLocation ?? throw new DomainException("Task could not be located");

        if (!await _messageManager.DeleteTaskMessageAsync(taskLocation))
        {
            throw new DomainException("Task could not be located");
        }

        await _taskRepository.DeleteTaskByIDAsync(taskID);

        _jobScheduler.CancelAllJobs(taskItem.ID);
    }

    public async Task ChangeTaskState(string taskID, TaskState taskState, ulong requesterID)
    {
        TaskItem taskItem = await GetTaskAsync(taskID);

        if (requesterID != taskItem.AssigneeID && !await _userInformationService.IsAdminAsync(requesterID))
        {
            throw new DomainException("You can not edit tasks that are not yours");
        }

        taskItem.ChangeState(taskState);

        var messageBuilder = _messageDirector.BuildTaskMessage(taskItem, GetTaskStatus(taskItem));
        var location = taskItem.TaskLocation ?? throw new DomainException("Task could not be located");

        if (taskItem.State != TaskState.ARCHIVED)
        {
            await messageBuilder.EditExistingMessage(location);
        }
        else
        {
            await _messageManager.DeleteTaskMessageAsync(location);
            taskItem.SetLocation(await messageBuilder.SendToChannel(_behaviorConfig.ArchiveChannelID));
        }

        await _taskRepository.UpdateTaskAsync(taskItem);

        if (taskItem.State >= TaskState.COMPLETED)
            _jobScheduler.CancelAllJobs(taskItem.ID);
    }

    public async Task RescheduleTask(string taskID, DateTime newDate, ulong requesterID)
    {
        await EnsureAdminAsync(requesterID);

        TaskItem taskItem = await GetTaskAsync(taskID);

        taskItem.TaskDuration.Reschedule(newDate);

        await UpdateTaskMessage(taskItem);

        await _taskRepository.UpdateTaskAsync(taskItem);

        _jobScheduler.CancelAllJobs(taskID);
        ScheduleTaskJobs(taskItem);
    }

    public async Task RescheduleTask(string taskID, int daysToAdd, ulong requesterID)
    {
        await EnsureAdminAsync(requesterID);

        TaskItem taskItem = await GetTaskAsync(taskID);

        taskItem.TaskDuration.Reschedule(daysToAdd);

        await UpdateTaskMessage(taskItem);

        await _taskRepository.UpdateTaskAsync(taskItem);

        _jobScheduler.CancelAllJobs(taskID);
        ScheduleTaskJobs(taskItem);
    }

    private async Task<TaskItem> GetTaskAsync(string taskID)
    {
        return await _taskRepository.GetTaskByIDAsync(taskID) ?? throw new DomainException("Task was not found");
    }

    private async Task EnsureAdminAsync(ulong userID)
    {
        if (!await _userInformationService.IsAdminAsync(userID))
        {
            throw new DomainException("Only administrators can perform that action");
        }
    }

    private TaskStatus GetTaskStatus(TaskItem taskItem)
    {
        var dateOffset = taskItem.TaskDuration.UtcDueDateOffset(DateTime.UtcNow);
        if (dateOffset <= TimeSpan.Zero)
        {
            return TaskStatus.AFTER_DEADLINE;
        }
        else if (dateOffset <= _behaviorConfig.TimespanBeforeAlert)
        {
            return TaskStatus.NEAR_DEADLINE;
        }
        else
        {
            return TaskStatus.BEFORE_DEADLINE;
        }
    }

    private async Task UpdateTaskMessage(TaskItem taskItem)
    {
        var messageBuilder = _messageDirector.BuildTaskMessage(taskItem, GetTaskStatus(taskItem));
        var location = taskItem.TaskLocation ?? throw new DomainException("Task could not be located");

        await messageBuilder.EditExistingMessage(location);
    }

    private void ScheduleTaskJobs(TaskItem taskItem)
    {
        var utcDueDate = taskItem.TaskDuration.UtcDueDate;

        var utcDeadlineDate = utcDueDate.Add(_behaviorConfig.DeadlineTolerance);
        var utcAlertDate = utcDeadlineDate.Subtract(_behaviorConfig.TimespanBeforeAlert);
        // set alert
        _jobScheduler.ScheduleJob(taskItem.ID, taskItem.TaskDuration.UtcDueDate.Add(_behaviorConfig.DeadlineTolerance), async () => await UpdateTaskMessage(taskItem));

        // set deadline
        _jobScheduler.ScheduleJob(taskItem.ID, taskItem.TaskDuration.UtcDueDate.Add(_behaviorConfig.DeadlineTolerance), async () => await UpdateTaskMessage(taskItem));
    }
}