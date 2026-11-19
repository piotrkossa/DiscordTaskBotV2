namespace DiscordTaskBot.Application;

using DiscordTaskBot.Domain;
using MediatR;

public enum TaskProgressAction
{
    // The requester marks their own progress.
    Start,
    Complete,
    // Archives the whole task (only once everyone has completed it).
    Archive
}

public record UpdateTaskProgressCommand(Guid TaskId, TaskProgressAction Action, ulong RequesterID) : IRequest<TaskItem>;
