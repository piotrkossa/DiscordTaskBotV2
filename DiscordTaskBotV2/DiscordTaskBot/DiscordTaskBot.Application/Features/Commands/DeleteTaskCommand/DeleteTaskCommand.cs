namespace DiscordTaskBot.Application;

using DiscordTaskBot.Domain;
using MediatR;

public record DeleteTaskCommand(Guid TaskId, ulong RequesterID) : IRequest<TaskItem>;