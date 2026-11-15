namespace DiscordTaskBot.Presentation;

using DiscordTaskBot.Application;
using Discord;
using Discord.Interactions;
using MediatR;
using DiscordTaskBot.Domain;
using Microsoft.Extensions.Logging;

public class AddTaskModule(IMediator mediator, ILogger<AddTaskModule> logger, ITaskRepository repo) : BaseCommand(mediator, logger)
{
    // Discord allows at most 25 options in a select menu.
    private const int MaxSelectOptions = 25;

    private readonly ITaskRepository _repo = repo;


    [SlashCommand("addtime", "Adds time to an existing task")]
    [RequireUserPermission(GuildPermission.Administrator)]
    public async Task AddTime(
        [Summary("days", "Days allocated to complete the task")] int days)
    {
        await base.ExecuteWithHandlingAsync(async () =>
        {
            await DeferAsync(ephemeral: true);

            var tasks = (await _repo.GetAllAsync()).ToList();

            // A select menu with no options is rejected by Discord.
            if (tasks.Count == 0)
            {
                await FollowupAsync("There are no tasks to add time to.", ephemeral: true);
                return;
            }

            var menuBuilder = new SelectMenuBuilder()
                .WithCustomId($"admin-addtime-select:{days}:{Context.Interaction.Id}")
                .WithPlaceholder("Pick task to add time...");

            // Discord rejects the whole menu above 25 options.
            foreach (var task in tasks.Take(MaxSelectOptions))
            {
                var label = string.IsNullOrEmpty(task.Description)
                    ? task.Id.ToString()
                    : task.Description.Substring(0, Math.Min(task.Description.Length, 100));
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
    public async Task HandleAdminTimeAddSelection(string daysString, string interactionId, string[] selectedTaskIds)
    {
        await base.ExecuteWithHandlingAsync(async () =>
        {
            await DeferAsync(ephemeral: true);

            if (selectedTaskIds.Length == 0 || !Guid.TryParse(selectedTaskIds[0], out var taskId))
                throw new ArgumentException("Invalid task selection");

            if (!int.TryParse(daysString, out var daysToAdd))
                throw new ArgumentException($"Invalid days value {daysString}");

            await _mediator.Send(new AddTimeCommand(taskId, daysToAdd, Context.User.Id));

            // The old code then called component.Message.ModifyAsync on the select menu. That message is
            // ephemeral, and Discord cannot edit it through the channel endpoint (404), so the user got
            // "An unexpected error occurred" after the time had already been saved. The new deadline shows
            // up on the task message the next time it is rebuilt (on the next state change), as before.
            await FollowupAsync("Time added", ephemeral: true);
        });
    }
}
