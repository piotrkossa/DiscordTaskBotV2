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

public class AddTaskModule(IMediator mediator, ILogger<AddTaskModule> logger, IOptions<DiscordBotOptions> options, DiscordSocketClient client, ITaskRepository repo) : BaseCommand(mediator, logger)
{
    private readonly DiscordBotOptions _botOptions = options.Value;
    private readonly DiscordSocketClient _client = client;
    private readonly ITaskRepository _repo = repo;


    [SlashCommand("addtime", "Adds time to an existing task")]
    [RequireUserPermission(GuildPermission.Administrator)]
    public async Task AddTime(
        [Summary("days", "Days allocated to complete the task")] int days)
    {
        await base.ExecuteWithHandlingAsync(async () =>
        {
            await DeferAsync(ephemeral: true);

            var tasks = await _repo.GetAllAsync();

            var menuBuilder = new SelectMenuBuilder()
                .WithCustomId($"admin-addtime-select:{days}:{Context.Interaction.Id}")
                .WithPlaceholder("Pick task to add time...");

            foreach (var task in tasks)
            {
                var label = task.Description.Substring(0, Math.Min(task.Description.Length, 100));
                menuBuilder.AddOption(label, task.Id.ToString());
            }


            var component = new ComponentBuilder().WithSelectMenu(menuBuilder).Build();

            await FollowupOrRespond(ephemeral: true, configureMessage: props =>
            {
                props.Content = "Choose the task to add time:";
                props.Components = component;
            });
    
        });

    }

    [ComponentInteraction("admin-addtime-select:*:*")]
    public async Task HandleAdminTimeAddSelection(string daysString, string location, string[] selectedTaskIds)
    {
        await base.ExecuteWithHandlingAsync(async () =>
        {
            await DeferAsync(ephemeral: true);

            var taskId = Guid.Parse(selectedTaskIds[0]);

            var daysToAdd = int.Parse(daysString);
            var messageId = ulong.Parse(location);

            var task = await mediator.Send(new AddTimeCommand(taskId, daysToAdd, Context.User.Id));

            var component = Context.Interaction as SocketMessageComponent;

            if (component != null)
            {
                await component.Message.ModifyAsync(new DiscordTaskMessageDirector(task).BuildByState(task.State));
            }
            else
            {
                throw new InfrastructureException("Could not get message component from interaction");
            }

            await FollowupAsync("Time added", ephemeral: true);
        });
    }

}