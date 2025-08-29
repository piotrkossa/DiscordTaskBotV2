namespace DiscordTaskBot.Domain;

public record TaskStateUpdatedEvent(Guid TaskID, TaskState TaskState) : IDomainEvent;