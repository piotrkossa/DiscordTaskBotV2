using Discord;
using Discord.WebSocket;
using DiscordTaskBot.Models;

namespace DiscordTaskBot.Services
{
    public class ButtonHandlerService
    {
        private readonly TaskService _taskService;
        private readonly DiscordService _discordService;

        public ButtonHandlerService(TaskService taskService, DiscordService discordService)
        {
            _taskService = taskService;
            _discordService = discordService;
        }

        public async Task HandleButton(SocketMessageComponent component)
        {
            var parts = component.Data.CustomId.Split('_');

            await component.DeferAsync(true);

            if (parts.Length != 2)
            {
                await component.FollowupAsync("Invalid button format.", ephemeral: true);
                return;
            }

            var action = parts[0];
            var taskID = parts[1];

            var taskData = await _taskService.GetTaskByIDAsync(taskID);
            if (taskData == null)
            {
                await component.FollowupAsync("Task not found.", ephemeral: true);
                return;
            }

            var message = component.Message as IUserMessage;

            if (component.User is not SocketGuildUser user)
            {
                await component.FollowupAsync("Could not indentify user.", ephemeral: true);
                return;
            }

            switch (action)
            {
                case "state":
                    if (taskData.State < TaskStates.COMPLETED)
                    {
                        if (user.Id != taskData.UserID && !user.GuildPermissions.Administrator)
                        {
                            await component.FollowupAsync("It is not your task!", ephemeral: true);
                            return;
                        }
                        await _taskService.IncreaseTaskState(taskID);

                        (var embed, var components) = BuilderService.BuildMessage(taskData, taskID);

                        await _discordService.UpdateMessageAsync(embed, components, message);
                    }
                    else if (taskData.State == TaskStates.COMPLETED)
                    {
                        if (!user.GuildPermissions.Administrator)
                        {
                            await component.FollowupAsync("You do not have permissions!", ephemeral: true);
                            return;
                        }
                        await _taskService.IncreaseTaskState(taskID);
                        taskData = await _taskService.GetTaskByIDAsync(taskID);

                        (var embed, var components) = BuilderService.BuildMessage(taskData!, taskID);

                        await _discordService.UpdateMessageAsync(embed, components, message);
                        var movedMessage = await _discordService.MoveMessageAsync(component);

                        if (movedMessage == null)
                        {
                            await component.FollowupAsync("Failed to move task to archive.", ephemeral: true);
                            return;
                        }
                        
                        await _taskService.UpdateTaskLocationAsync(taskID, movedMessage.Channel.Id, movedMessage.Id);

                        await component.FollowupAsync("Task moved to archive,", ephemeral: true);
                    }
                    break;
                case "delete":
                    if (!user.GuildPermissions.Administrator)
                    {
                        await component.FollowupAsync("You do not have permissions!", ephemeral: true);
                        return;
                    }
                    await _taskService.RemoveTask(taskID);
                    await component.Message.DeleteAsync();
                    await component.FollowupAsync("Task deleted.", ephemeral: true);
                    return;
            }
        }
    }
}