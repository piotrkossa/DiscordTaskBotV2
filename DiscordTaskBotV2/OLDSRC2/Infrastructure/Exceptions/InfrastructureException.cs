namespace DiscordTaskBot.Infrastructure;

public class InfrastructureException : Exception
{
    public InfrastructureException(string message) : base(message) { }
}