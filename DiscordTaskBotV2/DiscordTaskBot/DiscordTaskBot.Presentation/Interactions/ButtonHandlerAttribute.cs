namespace DiscordTaskBot.Presentation;

public enum DiscordButtonAction
{
    DELETE,
    RAISE_STATE
}

[AttributeUsage(AttributeTargets.Method, AllowMultiple = false)]
public class ButtonHandlerAttribute(DiscordButtonAction action) : Attribute
{
    public DiscordButtonAction Action { get; } = action;
}
