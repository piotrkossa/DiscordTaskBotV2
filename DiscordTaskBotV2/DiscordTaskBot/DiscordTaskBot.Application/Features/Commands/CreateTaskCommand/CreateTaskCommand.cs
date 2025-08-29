namespace DiscordTaskBot.Application;

using DiscordTaskBot.Domain;
using MediatR;

public record CreateTaskCommand(string Description, TaskDuration TaskDuration, ulong AsigneeId, ulong RequesterID) : IRequest<TaskItem>;