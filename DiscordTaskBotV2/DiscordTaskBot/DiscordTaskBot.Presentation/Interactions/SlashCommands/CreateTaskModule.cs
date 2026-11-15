namespace DiscordTaskBot.Presentation;

using DiscordTaskBot.Application;
using Discord;
using Discord.Interactions;
using MediatR;
using DiscordTaskBot.Domain;
using Microsoft.Extensions.Logging;

public class CreateTaskModule(IMediator mediator, ILogger<CreateTaskModule> logger) : BaseCommand(mediator, logger)
{
    [SlashCommand("createtask", "Creates new task")]
    public async Task CreateTask(
        [Summary("description", "Description of the task")] string description,
        [Summary("user", "User to whom the task will be assigned")] IUser user,
        [Summary("daysToDeadline", "Days allocated to complete the task")] int daysToDeadline)
    {
        await base.ExecuteWithHandlingAsync(async () =>
        {
            var utcNow = DateTime.UtcNow;

            var taskItem = await base._mediator.Send(new CreateTaskCommand(description, new TaskDuration(utcNow, utcNow.AddDays(daysToDeadline)), user.Id, Context.User.Id));

            await FollowupOrRespond(new DiscordTaskMessageDirector(taskItem).BuildNotStarted(), false);

            await NewTaskNotification(user, taskItem);
        });
    }

    // Uses the IUser from the command directly instead of looking the member up in the guild cache,
    // so the DM no longer fails when the member is not cached.
    private async Task NewTaskNotification(IUser user, TaskItem taskItem)
    {
        await base.ExecuteWithHandlingAsync(async () =>
        {
            await user.SendMessageAsync(
                "🎯 **New Task Alert!**\n\n" +
                $"**{taskItem.Description}**\n\n" +
                $"📅 Due: {DiscordUtility.GetDiscordTimestamp(taskItem.TaskDuration.UtcDueDate, 'R')}\n" +
                "📍 Check the tasks channel for complete information.");
        });
    }
}
