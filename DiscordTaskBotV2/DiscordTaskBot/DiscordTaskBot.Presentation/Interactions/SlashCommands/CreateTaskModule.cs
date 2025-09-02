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
            await DeferAsync();
            
            var response = await FollowupAsync("Creating Task...");

            var utcNow = DateTime.UtcNow;

            var taskItem = await base._mediator.Send(new CreateTaskCommand(description, new TaskDuration(utcNow, utcNow.AddDays(daysToDeadline)), user.Id, Context.User.Id));

            await response.ModifyAsync(msg =>
            {
                msg.Content = taskItem.Description;
            });
        });
    }
}