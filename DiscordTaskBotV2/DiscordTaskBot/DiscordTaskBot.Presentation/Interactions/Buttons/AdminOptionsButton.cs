namespace DiscordTaskBot.Presentation;

using Discord;
using Discord.Interactions;
using Discord.WebSocket;
using DiscordTaskBot.Application;
using MediatR;
using Microsoft.Extensions.Logging;


public static class AdminTaskOptions
{
    public const string Delete = "button_taskdelete";
    public const string AddTime = "button_taskaddtime";
}


public class AdminOptionsButton(IMediator mediator, ILogger<AdminOptionsButton> logger) : BaseCommand(mediator, logger)
{
    [ComponentInteraction(ButtonActions.TaskOptions + ":*")]
    public async Task ShowOptionsMenu(string taskId)
    {
        await base.ExecuteWithHandlingAsync(async () =>
        {
            var message = Context.Interaction as SocketMessageComponent 
                ?? throw new ArgumentException("Could not find message component from context");

            var messageId = message.Message.Id;
            var channelId = message.Channel.Id;

            
            var menuBuilder = new SelectMenuBuilder()
                // task-action-select:taskId:messageId:channelId
                .WithCustomId($"task-action-select:{taskId}:{messageId}:{channelId}")
                .WithPlaceholder("Choose Action...");

                menuBuilder.AddOption("Add Time", $"{AdminTaskOptions.AddTime}", "Open window to add time.");
                menuBuilder.AddOption("Delete Task", $"{AdminTaskOptions.Delete}", "Deletes task forever.");

            var component = new ComponentBuilder().WithSelectMenu(menuBuilder).Build();

            await FollowupAsync("Choose Action:", components: component, ephemeral: true);
        });
    }

    [ComponentInteraction("task-action-select:*")] 
    public async Task HandleActionSelect(string taskId, string messageId, string channelId, string[] values)
    {
        await base.ExecuteWithHandlingAsync(async () =>
        {
            var action = values.First();

            

            switch (action)
            {
                case AdminTaskOptions.AddTime:
                    await FollowupAsync($"You selected to add time to task {taskId}.", ephemeral: true);
                    break;

                case AdminTaskOptions.Delete:
                    await DeleteTask(taskId);
                    break;

                default:
                    await FollowupAsync("Unknown action selected.", ephemeral: true);
                    break;
            }
        });
    }

    public async Task DeleteTask(string taskId)
    {
        await base.ExecuteWithHandlingAsync(async () =>
        {
            await DeferAsync(ephemeral: true);
            if (!Guid.TryParse(taskId, out var result)) throw new ArgumentException($"Invalid task Id {taskId}");

            await base._mediator.Send(new DeleteTaskCommand(result, Context.User.Id));

            var component = (SocketMessageComponent)Context.Interaction;
            await component.Message.DeleteAsync();
        });
    }
}