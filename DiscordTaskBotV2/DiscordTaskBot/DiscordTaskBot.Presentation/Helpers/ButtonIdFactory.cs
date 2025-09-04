namespace DiscordTaskBot.Presentation;

public static class ButtonActions
{
    public const string TaskDelete = "button_taskdelete";
    public const string TaskRaiseState = "button_taskraisestate";
}


public static class ButtonIdFactory
{
    private const char Separator = ':';
    // template for button: button_action:taskId

    public static string TaskRaiseState(Guid taskId)
        => $"{ButtonActions.TaskRaiseState}{Separator}{taskId}";

    public static string TaskDelete(Guid taskId)
        => $"{ButtonActions.TaskDelete}{Separator}{taskId}";
}