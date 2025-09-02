using Discord.WebSocket;

namespace DiscordTaskBot.Presentation;

public class DeleteTaskButton
{
    [ButtonHandler(DiscordButtonAction.DELETE)]
    public async Task Handle(SocketMessageComponent component, string taskId)
    {
        
    }
}