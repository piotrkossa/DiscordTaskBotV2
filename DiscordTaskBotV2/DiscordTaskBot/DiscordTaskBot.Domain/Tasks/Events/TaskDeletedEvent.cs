namespace DiscordTaskBot.Domain;

public record TaskDeletedEvent(Guid TaskID) : IDomainEvent;