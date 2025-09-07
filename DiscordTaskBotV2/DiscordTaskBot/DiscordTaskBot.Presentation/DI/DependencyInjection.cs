namespace DiscordTaskBot.Presentation;

using System.Reflection;
using Microsoft.Extensions.DependencyInjection;

public static class DependencyInjection
{
    public static IServiceCollection AddPresentation(this IServiceCollection services)
    {
        var handlerType = typeof(IDiscordEventHandler);

        var handlerTypes = Assembly.GetExecutingAssembly().GetTypes()
            .Where(t => t.IsClass && !t.IsAbstract && handlerType.IsAssignableFrom(t))
            .ToList();

        foreach (var type in handlerTypes)
        {
            services.AddSingleton(handlerType, type);
        }

        Console.WriteLine($"[AUTO-REGISTRATION] Registered {handlerTypes.Count} event handlers:");
        foreach (var type in handlerTypes)
        {
            Console.WriteLine($"  - {type.Name}");
        }

        return services;
    }
}