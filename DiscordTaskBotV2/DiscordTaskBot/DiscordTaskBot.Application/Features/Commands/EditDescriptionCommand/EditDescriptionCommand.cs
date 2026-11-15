namespace DiscordTaskBot.Application;

using DiscordTaskBot.Domain;
using MediatR;

public record EditDescriptionCommand(Guid TaskId, string Description, ulong RequesterID) : IRequest<TaskItem>;
