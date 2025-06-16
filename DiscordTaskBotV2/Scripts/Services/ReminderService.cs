using Discord;
using Discord.WebSocket;
using DiscordTaskBot.Models;

namespace DiscordTaskBot.Services
{
    public class ReminderService
    {
        private readonly DiscordSocketClient _client;

        public ReminderService(DiscordSocketClient client)
        {
            _client = client;
        }

        public async Task SendNewTaskInfo(TaskData taskData)
        {
            string channelName = "unknown channel";

            if (await _client.GetChannelAsync(taskData.ChannelID) is not ISocketMessageChannel channel)
                Console.WriteLine($"Channel with ID: {taskData.ChannelID} not found!");
            else
                channelName = channel.Name;

            await SendMessage(taskData.UserID, $"Hello, you have been assigned a new task! Check **#{channelName}** for more information.");
        }

        public async Task SendReminder(TaskData taskData)
        {
            var daysLeft = (taskData.CompletionDate - DateTime.Now.Date).Days;
            string deadlineText = daysLeft switch
            {
                > 1 => $"{daysLeft} days left",
                1 => "1 day left",
                0 => "today is the deadline!",
                < 0 => $"overdue by {-daysLeft} days",
            };

            string message = $"🔔 Reminder: You still have a task assigned!\n" +
                            $"**Description:** {taskData.Description}\n" +
                            $"**Deadline:** {taskData.CompletionDate:yyyy-MM-dd} ({deadlineText})";
            
            await SendMessage(taskData.UserID, message);

        }

        private async Task SendMessage(ulong userID, string text)
        {
            var user = await _client.GetUserAsync(userID);
            if (user == null)
            {
                Console.WriteLine($"User with ID: {userID} not found!");
                return;
            }

            try
            {
                var dm = await user.CreateDMChannelAsync();

                await dm.SendMessageAsync(text);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error when sending message to {user.Username}: {ex.Message}");
            }
        }
    }
}