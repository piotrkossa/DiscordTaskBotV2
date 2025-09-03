using Microsoft.EntityFrameworkCore.Query.SqlExpressions;

namespace DiscordTaskBot.Presentation;

using System.ComponentModel;
using Discord;
using DiscordTaskBot.Domain;

public class DiscordTaskMessageDirector(TaskItem taskItem)
{
    private readonly TaskItem _taskItem = taskItem;

    public MessageProperties BuildByState(TaskState state)
    {
        return state switch
        {
            TaskState.NOT_STARTED => BuildNotStarted(),
            TaskState.IN_PROGRESS => BuildInProgress(),
            TaskState.COMPLETED => BuildCompleted(),
            TaskState.ARCHIVED => BuildArchived(),
            _ => throw new ArgumentException($"State {state} is not handled")
        };
    }

    public MessageProperties BuildNotStarted()
    {
        MessageProperties messageProperties = new();

        messageProperties.Embed = CreateEmbed(Color.LightGrey);

        messageProperties.Components = new ComponentBuilder().WithButton(GetDeleteButton());

        return messageProperties;
    }
    public MessageProperties BuildInProgress()
    {
        MessageProperties messageProperties = new();

        messageProperties.Embed = CreateEmbed(Color.Orange);

        return messageProperties;
    }
    public MessageProperties BuildCompleted()
    {
        MessageProperties messageProperties = new();

        messageProperties.Embed = CreateEmbed(Color.Green);

        return messageProperties;
    }
    public MessageProperties BuildArchived()
    {
        MessageProperties messageProperties = new();

        messageProperties.Embed = CreateEmbed(Color.Purple);

        return messageProperties;
    }

    private ButtonComponent GetDeleteButton()
    {
        return new ButtonBuilder()
            .WithLabel("Delete")
            .WithStyle(ButtonStyle.Danger)
            .WithCustomId(ButtonIdFactory.TaskDelete(_taskItem.Id))
            .Build();
    }

    private Embed CreateEmbed(Color color)
    {
        return new EmbedBuilder()
            .WithTitle("Task")
            .WithDescription($"{_taskItem.Description}")
            .AddField("Assigned To", $"<@{_taskItem.AssigneeID}>", inline: true)  // mention the user
            .AddField("Deadline", DiscordUtility.GetDiscordTimestamp(_taskItem.TaskDuration.UtcDueDate, 'R'), inline: true)  // discord timestamp
            .AddField("Status", GetNicerStateName(), inline: true)
            .WithColor(color)
            .WithFooter(footer => footer.Text =
                $"Created on: {DiscordUtility.GetDiscordTimestamp(_taskItem.TaskDuration.UtcCreationDate, 'd')}")
            .Build();
    }

    private string GetNicerStateName()
    {
        return _taskItem.State switch
        {
            TaskState.NOT_STARTED => "Not Started",
            TaskState.IN_PROGRESS => "In Progress",
            TaskState.COMPLETED => "Completed",
            TaskState.ARCHIVED => "Archived",
            _ => throw new ArgumentException($"State {_taskItem.State} is not handled")
        };
    }
}