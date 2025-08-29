namespace DiscordTaskBot.Domain;

public record TaskCreatedEvent(TaskItem TaskItem) : IDomainEvent;