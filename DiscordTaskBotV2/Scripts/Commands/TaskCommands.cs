using Discord;
using Discord.Interactions;
using DiscordTaskBot.Helpers;
using DiscordTaskBot.Models;
using DiscordTaskBot.Services;

namespace DiscordTaskBot.Commands
{
    public class TaskCommands : InteractionModuleBase<SocketInteractionContext>
    {
        private readonly TaskService _taskService;
        private readonly ReminderService _reminderService;

        public TaskCommands(TaskService taskService, ReminderService reminderService)
        {
            _taskService = taskService;
            _reminderService = reminderService;
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
    }
}