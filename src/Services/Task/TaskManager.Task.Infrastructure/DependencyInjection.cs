using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using TaskManager.Shared.Infrastructure.Extensions;
using TaskManager.Task.Domain.Interfaces;
using TaskManager.Task.Infrastructure.Persistence;

namespace TaskManager.Task.Infrastructure;

public static class DependencyInjection
{
    public static IServiceCollection AddTaskInfrastructure(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        services.AddMongoDb(configuration);
        services.AddRedisCache(configuration);
        services.AddKafkaProducer(configuration);
        services.AddJwtAuthentication(configuration);

        services.AddScoped<ITaskItemRepository, TaskItemRepository>();

        return services;
    }
}
