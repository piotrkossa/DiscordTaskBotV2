namespace DiscordTaskBot.Presentation;

using Discord;
using DiscordTaskBot.Domain;

public class DiscordTaskMessageDirector(TaskItem taskItem)
{
    private readonly TaskItem _taskItem = taskItem;

    public Action<MessageProperties> BuildByState(TaskState state)
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

    public Action<MessageProperties> BuildNotStarted()
    {
        return msg =>
        {
            msg.Content = string.Join(" ", _taskItem.AssigneeIDs.Select(id => $"<@{id}>"));
            msg.Embed = CreateEmbed(Color.LightGrey);

            var builder = new ComponentBuilder();
            builder.WithButton("Start", ButtonIdFactory.TaskStart(_taskItem.Id), ButtonStyle.Secondary);
            AddOptionsButton(builder);
            msg.Components = builder.Build();
        };
    }
    public Action<MessageProperties> BuildInProgress()
    {
        return msg =>
        {
            msg.Content = null;
            msg.Embed = CreateEmbed(Color.Orange);

            var builder = new ComponentBuilder();
            // Others can still start while some are already working.
            if (_taskItem.AnyoneNotStarted)
                builder.WithButton("Start", ButtonIdFactory.TaskStart(_taskItem.Id), ButtonStyle.Secondary);
            builder.WithButton("Complete", ButtonIdFactory.TaskComplete(_taskItem.Id), ButtonStyle.Success);
            AddOptionsButton(builder);
            msg.Components = builder.Build();
        };
    }
    public Action<MessageProperties> BuildCompleted()
    {
        return msg =>
        {
            msg.Content = null;
            msg.Embed = CreateEmbed(Color.Green);

            var builder = new ComponentBuilder();
            builder.WithButton("Archive", ButtonIdFactory.TaskArchive(_taskItem.Id), ButtonStyle.Primary);
            AddOptionsButton(builder);
            msg.Components = builder.Build();
        };
    }
    public Action<MessageProperties> BuildArchived()
    {
        return msg =>
        {
            msg.Content = null;
            msg.Embed = CreateEmbed(Color.Purple);
            msg.Components = null;
        };
    }

    private void AddOptionsButton(ComponentBuilder builder)
    {
        builder.WithButton("Options", ButtonIdFactory.TaskOptions(_taskItem.Id), ButtonStyle.Secondary);
    }

    private bool HasMultipleAssignees => _taskItem.AssigneeIDs.Count > 1;

    private Embed CreateEmbed(Color color)
    {
        return new EmbedBuilder()
            .WithTitle("Task")
            .WithDescription($"{_taskItem.Description}")
            .AddField("Assigned To", GetAssigneesText(), inline: true)
            .AddField("Deadline", DiscordUtility.GetDiscordTimestamp(_taskItem.TaskDuration.UtcDueDate, 'R'), inline: true)  // discord timestamp
            .AddField("State", GetNicerStateName(), inline: true)
            .AddField("Created on", DiscordUtility.GetDiscordTimestamp(_taskItem.TaskDuration.UtcCreationDate, 'd'))
            .WithColor(color)
            .Build();
    }

    // One assignee: just the mention, as before. Several: one line per person with their own progress.
    private string GetAssigneesText()
    {
        if (!HasMultipleAssignees)
            return string.Join(", ", _taskItem.AssigneeIDs.Select(id => $"<@{id}>"));

        return string.Join("\n", _taskItem.AssigneeIDs.Select(id => $"{ProgressIcon(_taskItem.GetProgress(id))} <@{id}>"));
    }

    private static string ProgressIcon(AssigneeProgress progress) => progress switch
    {
        AssigneeProgress.Completed => "✅",
        AssigneeProgress.InProgress => "🟠",
        _ => "⚪"
    };

    private string GetNicerStateName()
    {
        var name = _taskItem.State switch
        {
            TaskState.NOT_STARTED => "Not Started",
            TaskState.IN_PROGRESS => "In Progress",
            TaskState.COMPLETED => "Completed",
            TaskState.ARCHIVED => "Archived",
            _ => throw new ArgumentException($"State {_taskItem.State} is not handled")
        };

        // e.g. "In Progress (1/3 done)"
        if (HasMultipleAssignees && _taskItem.State == TaskState.IN_PROGRESS)
            name += $" ({_taskItem.CompletedAssigneeIDs.Count}/{_taskItem.AssigneeIDs.Count} done)";

        return name;
    }
}
