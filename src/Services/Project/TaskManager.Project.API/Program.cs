using Scalar.AspNetCore;
using Serilog;
using TaskManager.Project.Infrastructure;
using TaskManager.Shared.Infrastructure.Extensions;

var builder = WebApplication.CreateBuilder(args);

builder.Host.UseSerilog((context, config) =>
{
    config
        .ReadFrom.Configuration(context.Configuration)
        .WriteTo.Console()
        .Enrich.FromLogContext();
});

builder.Services.AddControllers();
builder.Services.AddOpenApi();
builder.Services.AddHealthChecks();

builder.Services.AddApplicationServices(
    typeof(TaskManager.Project.Application.Commands.CreateProject.CreateProjectCommand).Assembly);

builder.Services.AddProjectInfrastructure(builder.Configuration);

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
    app.MapScalarApiReference(options =>
    {
        options.Title = "TaskManager Project API";
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

public partial class Program { }
