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
            msg.Content = $"<@{_taskItem.AssigneeID}>";
            msg.Embed = CreateEmbed(Color.LightGrey);

            var builder = new ComponentBuilder();
            builder.WithButton("Start", ButtonIdFactory.TaskRaiseState(_taskItem.Id), ButtonStyle.Secondary);
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
            builder.WithButton("Complete", ButtonIdFactory.TaskRaiseState(_taskItem.Id), ButtonStyle.Success);
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
            builder.WithButton("Archive", ButtonIdFactory.TaskRaiseState(_taskItem.Id), ButtonStyle.Primary);
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

    private Embed CreateEmbed(Color color)
    {
        return new EmbedBuilder()
            .WithTitle("Task")
            .WithDescription($"{_taskItem.Description}")
            .AddField("Assigned To", $"<@{_taskItem.AssigneeID}>", inline: true)  // mention the user
            .AddField("Deadline", DiscordUtility.GetDiscordTimestamp(_taskItem.TaskDuration.UtcDueDate, 'R'), inline: true)  // discord timestamp
            .AddField("State", GetNicerStateName(), inline: true)
            .AddField("Created on", DiscordUtility.GetDiscordTimestamp(_taskItem.TaskDuration.UtcCreationDate, 'd'))
            .WithColor(color)
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