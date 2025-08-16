namespace DiscordTaskBot.Core;

public interface IDiscordMessageService
{
    // returns task location
    Task<TaskLocation> CreateTaskMessageAsync(ulong channelID, TaskItem taskItem);
    // returns new task location
    Task<TaskLocation> MoveTaskMessageToArchiveAsync(TaskItem taskItem);
    // deletes task message, returns true if successful
    Task<bool> DeleteTaskMessageAsync(TaskLocation taskLocation);
}