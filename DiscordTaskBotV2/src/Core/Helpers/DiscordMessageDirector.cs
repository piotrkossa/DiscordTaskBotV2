namespace DiscordTaskBot.Core;

public class DiscordMessageDirector(IDiscordMessageBuilder discordMessageBuilder, IDiscordMessageUtility discordMessageUtility, AppearanceConfig appearanceConfig)
{
    private readonly IDiscordMessageBuilder _discordMessageBuilder = discordMessageBuilder;
    private readonly IDiscordMessageUtility _discordMessageUtility = discordMessageUtility;

    private readonly AppearanceConfig _appearanceConfig = appearanceConfig;

    public IDiscordMessageBuilder BuildTaskMessage(TaskItem taskItem, TaskStatus taskStatus)
    {
        _discordMessageBuilder.SetTitle("Task")
            .SetDescription(taskItem.Description)
                .AddField("Assigned To", _discordMessageUtility.GetUserMention(taskItem.AssigneeID), inline: true)
                .AddField("Deadline", _discordMessageUtility.GetDiscordTimestamp(taskItem.TaskDuration.UtcDueDate), inline: true)
                .AddField("State", GetStateName(taskItem.State), inline: true)
                .AddFooter($"Created on: {taskItem.TaskDuration.LocalCreationDate(_appearanceConfig.TimeZone):dd/MM/yyyy}")
                .SetColor(GetStateColor(taskItem.State));

        if (taskStatus == TaskStatus.AFTER_DEADLINE)
        {
            _discordMessageBuilder
                .SetTitle(_appearanceConfig.DeadlineEmoji + " Task")
                .SetColor(_appearanceConfig.DeadlineColor);
        }
        else if (taskStatus == TaskStatus.NEAR_DEADLINE)
        {
            _discordMessageBuilder
                .SetTitle(_appearanceConfig.AlertEmoji + " Task")
                .SetColor(_appearanceConfig.AlertColor);
        }

        return _discordMessageBuilder;
    }

    private string GetStateName(TaskState taskState)
    {
        return taskState switch
        {
            TaskState.NOT_STARTED => _appearanceConfig.NotStartedEmoji + " Not Started",
            TaskState.IN_PROGRESS => _appearanceConfig.InProgressEmoji + " In Progress",
            TaskState.COMPLETED => _appearanceConfig.CompletedEmoji + " Completed",
            TaskState.ARCHIVED => _appearanceConfig.ArchivedEmoji + " Archived",
            _ => throw new DomainException("Unknown state")
        };
    }

    private uint GetStateColor(TaskState taskState)
    {
        return taskState switch
        {
            TaskState.NOT_STARTED => _appearanceConfig.NotStartedColor,
            TaskState.IN_PROGRESS => _appearanceConfig.InProgressColor,
            TaskState.COMPLETED => _appearanceConfig.CompletedColor,
            TaskState.ARCHIVED => _appearanceConfig.ArchivedColor,
            _ => throw new DomainException("Unknown state")
        };
    }
}