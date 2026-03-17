using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using TaskManager.Notification.Domain.Interfaces;
using TaskManager.Notification.Infrastructure.Consumers;
using TaskManager.Notification.Infrastructure.Persistence;
using TaskManager.Shared.Infrastructure.Extensions;

namespace TaskManager.Notification.Infrastructure;

public static class DependencyInjection
{
    public static IServiceCollection AddNotificationInfrastructure(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        services.AddMongoDb(configuration);
        services.AddRedisCache(configuration);
        services.AddJwtAuthentication(configuration);

        services.Configure<Shared.Infrastructure.Settings.KafkaSettings>(
            configuration.GetSection("Kafka"));

        services.AddScoped<INotificationRepository, NotificationRepository>();
        services.AddHostedService<NotificationConsumer>();

        return services;
    }
}
