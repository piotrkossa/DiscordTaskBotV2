namespace DiscordTaskBot.Infrastructure;

using DiscordTaskBot.Application;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

public static class DependencyInjection
{
    public static IServiceCollection AddInfrastructure(this IServiceCollection services)
    {

        services.AddDbContext<TaskItemDbContext>(options =>
        {
            options.UseSqlite("Data Source=taskitems.db");

            options.EnableSensitiveDataLogging();
            options.EnableDetailedErrors();
        });

        services.AddSingleton<IAuthorizationService, AuthorizationService>();
        services.AddSingleton<ITaskRepository, TaskRepository>();
        return services;
    }
}