using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using TaskManager.Project.Domain.Interfaces;
using TaskManager.Project.Infrastructure.Persistence;
using TaskManager.Shared.Infrastructure.Extensions;

namespace TaskManager.Project.Infrastructure;

public static class DependencyInjection
{
    public static IServiceCollection AddProjectInfrastructure(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        services.AddMongoDb(configuration);
        services.AddRedisCache(configuration);
        services.AddKafkaProducer(configuration);
        services.AddJwtAuthentication(configuration);

        services.AddScoped<IProjectRepository, ProjectRepository>();

        return services;
    }
}
