namespace DiscordTaskBot.Presentation;

public static class ButtonActions
{
    public const string TaskOptions = "button_taskoptions";
    public const string TaskStart = "button_taskstart";
    public const string TaskComplete = "button_taskcomplete";
    public const string TaskArchive = "button_taskarchive";

    // Single "next state" button used by messages posted before per-person progress.
    // Still handled so those messages keep working; they get the new buttons on the first click.
    public const string TaskRaiseState = "button_taskraisestate";
}


public static class ButtonIdFactory
{
    private const char Separator = ':';
    // template for button: button_action:taskId

    public static string TaskStart(Guid taskId)
        => $"{ButtonActions.TaskStart}{Separator}{taskId}";

    public static string TaskComplete(Guid taskId)
        => $"{ButtonActions.TaskComplete}{Separator}{taskId}";

    public static string TaskArchive(Guid taskId)
        => $"{ButtonActions.TaskArchive}{Separator}{taskId}";

    public static string TaskOptions(Guid taskId)
        => $"{ButtonActions.TaskOptions}{Separator}{taskId}";
}
