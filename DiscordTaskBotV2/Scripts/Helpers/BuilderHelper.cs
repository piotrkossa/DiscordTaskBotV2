using Discord;
using DiscordTaskBot.Models;

namespace DiscordTaskBot.Helpers
{
    public static class BuilderHelper
    {
        public static string GetDiscordTimestamp(DateTime dateTime, char format = 'R')
        {
            long unixTime = ((DateTimeOffset)dateTime.ToUniversalTime()).ToUnixTimeSeconds();
            return $"<t:{unixTime}:{format}>";
        }

        public static (Embed, MessageComponent?) BuildMessage(TaskData taskData, string taskID)
        {
            string stateName = "";
            string buttonName = "";
            string titleEmoji = "";
            Color embedColor = Color.Default;
            ButtonStyle buttonStyle = ButtonStyle.Secondary;

            var remainingTime = taskData.CompletionDate - DateTime.Now;

            var isLate = false;

            if (remainingTime.TotalHours <= 0)
            {
                titleEmoji = "💀 ";
                embedColor = new Color(0, 0, 0);
                isLate = true;
            }
            else if (remainingTime.TotalHours <= 24)
            {
                titleEmoji = "❗ ";
                embedColor = new Color(204, 0, 0);
                isLate = true;
            }

            switch (taskData.State)
            {
                case TaskStates.NOT_STARTED:
                    stateName = "⏳ Not Started";
                    buttonName = "▶️  Start";
                    if (!isLate)
                        embedColor = Color.LightGrey;
                    break;
                case TaskStates.IN_PROGRESS:
                    stateName = "🔨 In Progress";
                    buttonName = "🏁  Complete";
                    buttonStyle = ButtonStyle.Primary;
                    if (!isLate)
                        embedColor = Color.Orange;
                    break;
                case TaskStates.COMPLETED:
                    stateName = "✅ Completed";
                    buttonName = "📥  Archive";
                    embedColor = Color.Green;
                    buttonStyle = ButtonStyle.Success;
                    titleEmoji = "";
                    break;
                case TaskStates.ARCHIVED:
                    stateName = "📦 Archived";
                    embedColor = Color.Purple;
                    titleEmoji = "";
                    break;
            }

            var embed = new EmbedBuilder()
                .WithTitle(titleEmoji + "Task")
                .WithDescription($"{taskData.Description}")
                .AddField("Assigned To", $"<@{taskData.UserID}>", inline: true)  // mention the user
                                                                                 //.AddField("Deadline", taskData.CompletionDate.ToString("MM/dd/yyyy"), inline: true)  // nicer date format
                .AddField("Deadline", GetDiscordTimestamp(taskData.CompletionDate), inline: true)  // discord timestamp
                .AddField("Status", stateName, inline: true)
                .WithColor(embedColor)
                .WithFooter(footer => footer.Text = $"Created on: {taskData.CreationDate:dd/MM/yyyy}")
                .Build();


            if (taskData.State == TaskStates.ARCHIVED)
            {
                return (embed, null);
            }

            var component = new ComponentBuilder()
                .WithButton(buttonName, customId: $"state_{taskID}", buttonStyle)
                .WithButton("Cancel", customId: $"delete_{taskID}", ButtonStyle.Danger)
                .Build();

            return (embed, component);
        }
    }
}