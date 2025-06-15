using Discord;
using Discord.Interactions;

namespace DiscordTaskBot.Commands
{
    public class TaskCommands : InteractionModuleBase<SocketInteractionContext>
    {
        [SlashCommand("createtask", "Creates new task")]
        public async Task CreateTask(
            [Summary("description", "Description of the task")] string description,
            [Summary("user", "User to whom the task will be assigned")] IUser user,
            [Summary("daysToDeadline", "Days allocated to complete the task")] int daysToDeadline)
        {
            await RespondAsync($"Dodano zadanie: {description} dla {user} na {daysToDeadline} dni.");
        }
    }
}