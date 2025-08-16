using Discord;
using LiteDB;

namespace DiscordTaskBot.Models
{
    public enum TaskStates
    {
        NOT_STARTED,
        IN_PROGRESS,
        COMPLETED,
        ARCHIVED
    }

    public class TaskData
    {
        [BsonId]
        public string ID { get; set; }
        
        public string Description { get; set; }
        public ulong UserID { get; set; }

        public DateTime CreationDate { get; set; }
        public DateTime CompletionDate { get; set; }

        public TaskStates State { get; set; }

        public ulong ChannelID { get; set; }
        public ulong MessageID { get; set; }

        public static TaskData FromDiscord(string description, IUser user, int daysToDeadline, IUserMessage message)
        {
            return new TaskData(description, user.Id, DateTime.Today, DateTime.Today.AddDays(daysToDeadline + 1).AddSeconds(-1), TaskStates.NOT_STARTED, message.Channel.Id, message.Id);
        }

        public TaskData(string description, ulong userID, DateTime creationDate, DateTime completionDate, TaskStates state, ulong channelID, ulong messageID)
        {

            Description = description;
            UserID = userID;
            CreationDate = creationDate;
            CompletionDate = completionDate;
            State = state;
            ChannelID = channelID;
            MessageID = messageID;
            ID = Guid.NewGuid().ToString();
        }
    }
}