namespace DiscordTaskBot.Application;

using DiscordTaskBot.Domain;
using MediatR;

public record AddTimeCommand(Guid TaskId, int Days, ulong RequesterID) : IRequest<TaskItem>;