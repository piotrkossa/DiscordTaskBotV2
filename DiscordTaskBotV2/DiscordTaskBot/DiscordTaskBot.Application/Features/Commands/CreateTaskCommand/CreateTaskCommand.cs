namespace DiscordTaskBot.Application;

using DiscordTaskBot.Domain;
using MediatR;

public record CreateTaskCommand(string Description, TaskDuration TaskDuration, IReadOnlyList<ulong> AssigneeIds, ulong RequesterID) : IRequest<TaskItem>;
