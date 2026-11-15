namespace DiscordTaskBot.Application;

using DiscordTaskBot.Domain;
using MediatR;

public record ChangeAssigneesCommand(Guid TaskId, IReadOnlyList<ulong> AssigneeIds, ulong RequesterID) : IRequest<ChangeAssigneesResult>;

/// <param name="AddedAssigneeIds">Users that were not assigned before - they should be notified.</param>
public record ChangeAssigneesResult(TaskItem Task, IReadOnlyList<ulong> AddedAssigneeIds);
