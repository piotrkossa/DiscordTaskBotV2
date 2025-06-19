using Discord;
using Discord.Interactions;
using Discord.WebSocket;
using DiscordTaskBot.Helpers;
using DiscordTaskBot.Models;
using DiscordTaskBot.Services;

namespace DiscordTaskBot.Commands
{
    public class TaskCommands : InteractionModuleBase<SocketInteractionContext>
    {
        private readonly TaskService _taskService;
        private readonly ReminderService _reminderService;
        private readonly DiscordSocketClient _client;
        private readonly DiscordService _discordService;

        public TaskCommands(TaskService taskService, ReminderService reminderService, DiscordSocketClient client, DiscordService discordService)
        {
            _discordService = discordService;
            _taskService = taskService;
            _reminderService = reminderService;
            _client = client;
        }

        [SlashCommand("createtask", "Creates new task")]
        public async Task CreateTask(
            [Summary("description", "Description of the task")] string description,
            [Summary("user", "User to whom the task will be assigned")] IUser user,
            [Summary("daysToDeadline", "Days allocated to complete the task")] int daysToDeadline)
        {
            await DeferAsync();
            var response = await FollowupAsync("Creating Task...");

            var taskData = TaskData.FromDiscord(description, user, daysToDeadline, response);
            var taskID = await _taskService.AddTask(taskData);


            (var embed, var components) = BuilderHelper.BuildMessage(taskData, taskID);

            await response.ModifyAsync(msg =>
            {
                msg.Content = null;
                msg.Embed = embed;
                msg.Components = components;
            });

            await _reminderService.SendNewTaskInfo(taskData);
        }

        [SlashCommand("addtime", "Adds time to specified task")]
        public async Task AddTime(
            [Summary("days", "How many days do you want to add?")] int days)
        {
            await DeferAsync(true);

            var menu = new SelectMenuBuilder()
                .WithCustomId($"task_options:{days}")
                .WithPlaceholder("Choose a task")
                .WithMinValues(1)
                .WithMaxValues(1);

            foreach (var task in await _taskService.GetAllTasksAsync())
            {
                var user = await _client.GetUserAsync(task.Value.UserID);

                string description = task.Value.Description;
                description = description.Length > 30 ? description[..27] + "..." : description;

                menu.AddOption(user == null ? "unknown user" : $"@{user.GlobalName}", task.Key, description);
            }

            var component = new ComponentBuilder().WithSelectMenu(menu);

            await FollowupAsync("Choose a task:", components: component.Build(), ephemeral: true);
        }

        [ComponentInteraction("task_options:*")]
        public async Task HandleSelectMenu(string customID, string selected)
        {
            await DeferAsync(true);

            int daysToAdd = int.Parse(customID);
            var taskID = selected;


            var taskData = await _taskService.GetTaskByIDAsync(taskID);

            if (taskData == null) {
                await FollowupAsync($"Task not found!", ephemeral: true);
                return;
            }

            await _taskService.AddDaysToTask(taskID, daysToAdd);

            await _discordService.CreateAndUpdateMessageAsync(taskID, taskData);

            await FollowupAsync($"Added {daysToAdd} days to choosen task", ephemeral: true);
        }
    }
}