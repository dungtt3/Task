using Scalar.AspNetCore;
using Serilog;
using TaskManager.Auth.Infrastructure;
using TaskManager.Shared.Infrastructure.Extensions;

var builder = WebApplication.CreateBuilder(args);

// ─── Serilog ────────────────────────────────────────────
builder.Host.UseSerilog((context, config) =>
{
    config
        .ReadFrom.Configuration(context.Configuration)
        .WriteTo.Console()
        .Enrich.FromLogContext();
});

// ─── Services ───────────────────────────────────────────
builder.Services.AddControllers();
builder.Services.AddOpenApi();
builder.Services.AddHealthChecks();

// Application services (MediatR + FluentValidation)
builder.Services.AddApplicationServices(
    typeof(TaskManager.Auth.Application.Commands.Register.RegisterCommand).Assembly);

// Infrastructure (MongoDB, Redis, JWT)
builder.Services.AddAuthInfrastructure(builder.Configuration);

var app = builder.Build();

// ─── Middleware Pipeline ────────────────────────────────
if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
    app.MapScalarApiReference(options =>
    {
        options.Title = "TaskManager Auth API";
        options.Theme = ScalarTheme.BluePlanet;
    });
}
else
{
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseSharedMiddleware();

app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();
app.MapHealthChecks("/health");

app.Run();

// Make Program accessible for integration tests
public partial class Program { }
