namespace DiscordTaskBot.Presentation;

using DiscordTaskBot.Application;
using Discord;
using Discord.Interactions;
using MediatR;
using DiscordTaskBot.Domain;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Discord.WebSocket;
using DiscordTaskBot.Infrastructure;

public class CreateTaskModule(IMediator mediator, ILogger<CreateTaskModule> logger, IOptions<DiscordBotOptions> options, DiscordSocketClient client) : BaseCommand(mediator, logger)
{
    private readonly DiscordBotOptions _botOptions = options.Value;
    private readonly DiscordSocketClient _client = client;


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

            await NewTaskNotification(taskItem.AssigneeID, taskItem);
        });
    }

    private async Task NewTaskNotification(ulong userId, TaskItem taskItem)
    {
        await base.ExecuteWithHandlingAsync(async () =>
        {
            var guildId = _botOptions.GuildId;

            var guild = _client.GetGuild(guildId) ?? throw new InfrastructureException($"Guild with Id: {guildId} was not found!");

            var user = guild.GetUser(userId) ?? throw new InfrastructureException($"User with Id: {userId} was not found in guild with Id: {guildId}");


            await user.SendMessageAsync(
                "🎯 **New Task Alert!**\n\n" +
                $"**{taskItem.Description}**\n\n" +
                $"📅 Due: {GetDiscordDueDateString(taskItem.TaskDuration.UtcDueDate)}\n" +
                "📍 Check the tasks channel for complete information.");

        });
    }
    
    private string GetDiscordDueDateString(DateTime utcDueDate)
    {
        var unixTimestamp = ((DateTimeOffset)utcDueDate).ToUnixTimeSeconds();
        return $"<t:{unixTimestamp}:R>";
    }
}