namespace DiscordTaskBot.Infrastructure;

using DiscordTaskBot.Application;
using Microsoft.Extensions.DependencyInjection;

public static class DependencyInjection
{
    public static IServiceCollection AddInfrastructure(this IServiceCollection services)
    {
        services.AddDbContext<TaskItemDbContext>();

        services.AddSingleton<IAuthorizationService, AuthorizationService>();
        services.AddSingleton<ITaskRepository, TaskRepository>();
        return services;
    }
}