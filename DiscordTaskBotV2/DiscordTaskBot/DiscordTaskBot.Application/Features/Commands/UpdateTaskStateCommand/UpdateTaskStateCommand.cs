namespace DiscordTaskBot.Application;

using DiscordTaskBot.Domain;
using MediatR;

public record UpdateTaskStateCommand(Guid TaskId, TaskState TaskState, ulong RequesterID) : IRequest<TaskItem>;
