namespace DiscordTaskBot.Core;

public interface IDiscordMessageManager
{
    // deletes task message, returns true if successful
    Task<bool> DeleteTaskMessageAsync(TaskLocation taskLocation);
}