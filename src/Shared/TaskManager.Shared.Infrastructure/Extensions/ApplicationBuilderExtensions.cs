using Microsoft.AspNetCore.Builder;
using TaskManager.Shared.Infrastructure.Middleware;

namespace TaskManager.Shared.Infrastructure.Extensions;

public static class ApplicationBuilderExtensions
{
    public static IApplicationBuilder UseSharedMiddleware(this IApplicationBuilder app)
    {
        app.UseMiddleware<ExceptionMiddleware>();
        return app;
    }
}
