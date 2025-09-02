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

    public static string TaskRaiseState(string taskId)
        => $"{ButtonActions.TaskRaiseState}{Separator}{taskId}";

    public static string TaskDelete(string taskId)
        => $"{ButtonActions.TaskDelete}{Separator}{taskId}";


    public static bool TryParseTask(string customId, out string action, out string taskId)
    {
        action = "";
        taskId = "";

        var parts = customId.Split(Separator, 2);
        if (parts.Length != 2 || string.IsNullOrEmpty(action) || string.IsNullOrEmpty(taskId))
            return false;

        action = parts[0];
        taskId = parts[1];

        return true;
    }
}