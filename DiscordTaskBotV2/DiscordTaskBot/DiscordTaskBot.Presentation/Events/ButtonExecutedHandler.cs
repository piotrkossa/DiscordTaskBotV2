namespace DiscordTaskBot.Presentation;

using System.Reflection;
using Discord.WebSocket;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

public class ButtonExecutedHandler(
    ILogger<ButtonExecutedHandler> logger,
    DiscordSocketClient client,
    IServiceProvider services) : IDiscordEventHandler
{
    private readonly DiscordSocketClient _client = client;
    private readonly ILogger<ButtonExecutedHandler> _logger = logger;
    private readonly IServiceProvider _services = services;

    private readonly Dictionary<DiscordButtonAction, Func<SocketMessageComponent, string, Task>> _handlers = new();

    public void Initialize()
    {
        _client.ButtonExecuted += ButtonExecuted;

        RegisterHandlers(_services);
    }

    private async Task ButtonExecuted(SocketMessageComponent component)
    {
        try
        {
            if (DiscordUtility.TryParseButtonID(component.Data.CustomId, out var action, out var taskId) && _handlers.TryGetValue(action, out var handler))
            {
                await handler(component, taskId);
            }
            else
            {
                await component.RespondAsync("Unknown button", ephemeral: true);
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error handling button {CustomId}", component.Data.CustomId);
        }
    }

    private void RegisterHandlers(IServiceProvider services)
    {
        // get assembly
        var assembly = typeof(ButtonExecutedHandler).Assembly;

        // get all classes with [ButtonHandler]
        var handlerTypes = assembly.GetTypes()
            .Where(t => t.IsClass && !t.IsAbstract
                        && t.GetMethods(BindingFlags.Public | BindingFlags.Instance)
                                .Any(m => m.GetCustomAttribute<ButtonHandlerAttribute>() != null));


        foreach (var type in handlerTypes)
        {
            // create instance of every button handling class thru DI
            var instance = ActivatorUtilities.CreateInstance(services, type);

            // iterate over methods with the [ButtonHandler]
            foreach (var method in type.GetMethods(BindingFlags.Public | BindingFlags.Instance))
            {
                var attributes = method.GetCustomAttributes<ButtonHandlerAttribute>();
                foreach (var attribute in attributes)
                {
                    // create delegate for method
                    var del = (Func<SocketMessageComponent, string, Task>)
                        Delegate.CreateDelegate(typeof(Func<SocketMessageComponent, string, Task>), instance, method);

                    // register delegate in handlers
                    if (_handlers.ContainsKey(attribute.Action))
                    {
                        _logger.LogWarning("Button handler for '{Action}' already exists, overwriting.", attribute.Action);
                    }
                    _handlers[attribute.Action] = del;
                }
            }
        }
    }
}