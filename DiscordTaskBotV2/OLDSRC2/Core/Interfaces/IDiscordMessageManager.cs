namespace DiscordTaskBot.Core;

public interface IDiscordMessageManager
{
    // deletes task message, returns true if successful, false if not
    Task<bool> DeleteTaskMessageAsync(TaskLocation taskLocation);
}