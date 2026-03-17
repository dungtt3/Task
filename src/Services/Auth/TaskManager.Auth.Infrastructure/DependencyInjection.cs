using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using TaskManager.Auth.Application.Interfaces;
using TaskManager.Auth.Domain.Interfaces;
using TaskManager.Auth.Infrastructure.Persistence;
using TaskManager.Auth.Infrastructure.Services;
using TaskManager.Shared.Infrastructure.Extensions;

namespace TaskManager.Auth.Infrastructure;

public static class DependencyInjection
{
    public static IServiceCollection AddAuthInfrastructure(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        services.AddMongoDb(configuration);
        services.AddRedisCache(configuration);
        services.AddJwtAuthentication(configuration);

        services.AddScoped<IUserRepository, UserRepository>();
        services.AddScoped<IJwtTokenService, JwtTokenService>();

        return services;
    }
}
