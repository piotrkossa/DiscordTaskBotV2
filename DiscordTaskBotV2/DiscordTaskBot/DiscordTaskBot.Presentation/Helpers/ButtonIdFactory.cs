namespace DiscordTaskBot.Presentation;

public static class ButtonActions
{
    public const string TaskOptions = "button_taskoptions";
    public const string TaskRaiseState = "button_taskraisestate";
}


public static class ButtonIdFactory
{
    private const char Separator = ':';
    // template for button: button_action:taskId

    public static string TaskRaiseState(Guid taskId)
        => $"{ButtonActions.TaskRaiseState}{Separator}{taskId}";

    public static string TaskOptions(Guid taskId)
        => $"{ButtonActions.TaskOptions}{Separator}{taskId}";
}