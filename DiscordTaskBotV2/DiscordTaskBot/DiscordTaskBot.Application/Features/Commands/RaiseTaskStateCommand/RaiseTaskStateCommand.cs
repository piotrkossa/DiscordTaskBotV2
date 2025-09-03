namespace DiscordTaskBot.Application;

using DiscordTaskBot.Domain;
using MediatR;

public record RaiseTaskStateCommand(Guid TaskId, ulong RequesterID) : IRequest<TaskItem>;
