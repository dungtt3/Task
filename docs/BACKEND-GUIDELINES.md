# Backend Guidelines — ASP.NET Core

> Based on [OpenAI ASP.NET Core Skill](https://github.com/openai/skills/blob/main/skills/.curated/aspnet-core/SKILL.md)
> Target: **.NET 10 / ASP.NET Core 10** (latest stable as of March 2026)

This document defines mandatory backend standards for the Task Manager microservices.

---

## 1. Target Framework & Defaults

- **Framework:** .NET 10 / ASP.NET Core 10
- **App model:** Controller-based Web APIs with `[ApiController]`
- **Host:** `WebApplicationBuilder` + `WebApplication` (modern hosting model)
- **No `Startup` class** — everything in `Program.cs` with extension methods for organization
- **Built-in features first** before third-party: DI, options/configuration, logging, ProblemDetails, OpenAPI, health checks, rate limiting, output caching, Identity

## 2. Program.cs Structure (Per Service)

```csharp
var builder = WebApplication.CreateBuilder(args);

// ─── 1. Services Registration ───────────────────────────
builder.Services.AddControllers();
builder.Services.AddOpenApi();
builder.Services.AddHealthChecks();

// Feature-specific registrations (extracted to extension methods)
builder.Services.AddAuthenticationServices(builder.Configuration);
builder.Services.AddInfrastructureServices(builder.Configuration);
builder.Services.AddApplicationServices();

var app = builder.Build();

// ─── 2. Middleware Pipeline (ORDER MATTERS!) ────────────
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler();
    app.UseHsts();
}

app.UseHttpsRedirection();

// Custom middleware
app.UseMiddleware<ExceptionMiddleware>();

// Auth pipeline
app.UseAuthentication();
app.UseAuthorization();

// Rate limiting (built-in .NET 10)
app.UseRateLimiter();

// Endpoints
app.MapControllers();
app.MapHealthChecks("/health");

app.Run();
```

### Middleware Order (Strict)

```
1. Forwarded headers (if behind proxy)
2. Exception handling + HSTS (non-dev)
3. HTTPS redirection
4. CORS
5. Authentication          ← MUST be before Authorization
6. Authorization
7. Rate limiting
8. Custom middleware (cache, etc.)
9. Endpoint mapping
```

⚠️ **Never** insert custom middleware between `UseAuthentication()` and `UseAuthorization()` without reason.

## 3. Clean Architecture Layers

### Domain Layer (`*.Domain`)
- Entities, Value Objects, Enums, Domain Events
- **Zero dependencies** — no framework, no NuGet references
- Interfaces for repositories defined here, implemented in Infrastructure

### Application Layer (`*.Application`)
- CQRS via **MediatR** (Commands + Queries + Handlers)
- **FluentValidation** for request validation
- DTOs separate from persistence models
- Pipeline behaviors: Validation → Logging → Handler
- Interfaces for external services (Kafka, Cache)

### Infrastructure Layer (`*.Infrastructure`)
- MongoDB driver implementation of repositories
- Kafka producer/consumer
- Redis cache service
- External service integrations
- `DependencyInjection.cs` extension method for registration

### API Layer (`*.API`)
- Controllers (thin — delegate to MediatR)
- Middleware (auth, rate limit, cache, exception)
- `Program.cs`
- API-specific configuration

## 4. Controller Guidelines

```csharp
[ApiController]
[Route("api/v1/[controller]")]
[Authorize]
public class TasksController : ControllerBase
{
    private readonly IMediator _mediator;

    public TasksController(IMediator mediator)
    {
        _mediator = mediator;
    }

    [HttpGet]
    [ProducesResponseType(typeof(PagedResult<TaskDto>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetTasks([FromQuery] GetTasksQuery query)
    {
        var result = await _mediator.Send(query);
        return Ok(result);
    }

    [HttpPost]
    [ProducesResponseType(typeof(TaskDto), StatusCodes.Status201Created)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> CreateTask([FromBody] CreateTaskCommand command)
    {
        var result = await _mediator.Send(command);
        return CreatedAtAction(nameof(GetTaskById), new { id = result.Id }, result);
    }
}
```

### Controller Rules:
- Derive from `ControllerBase` (not `Controller`)
- Annotate with `[ApiController]`
- Use attribute routing: `[Route("api/v1/[controller]")]`
- **Thin controllers** — business logic lives in handlers/services
- Return `ProblemDetails` for errors (not ad hoc JSON)
- Use `[ProducesResponseType]` for OpenAPI documentation
- Apply `[Authorize]` at controller level, `[AllowAnonymous]` for specific endpoints
- `CreatedAtAction` for resource creation (201)
- Explicit status codes, not implicit

## 5. Dependency Injection

### Lifetime Rules:
| Lifetime | Use For |
|----------|---------|
| **Singleton** | Stateless infrastructure (Kafka producer, cache client) |
| **Scoped** | Request-bound work (MongoDB context, unit of work) |
| **Transient** | Lightweight stateless services |

### Registration Pattern:
```csharp
// In Infrastructure/DependencyInjection.cs
public static class DependencyInjection
{
    public static IServiceCollection AddInfrastructureServices(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        // MongoDB
        services.Configure<MongoDbSettings>(
            configuration.GetSection("MongoDB"));
        services.AddSingleton<IMongoClient>(sp =>
        {
            var settings = sp.GetRequiredService<IOptions<MongoDbSettings>>().Value;
            return new MongoClient(settings.ConnectionString);
        });
        services.AddScoped<ITaskRepository, TaskRepository>();

        // Redis
        services.AddSingleton<IConnectionMultiplexer>(sp =>
        {
            var config = configuration.GetConnectionString("Redis")!;
            return ConnectionMultiplexer.Connect(config);
        });
        services.AddScoped<ICacheService, RedisCacheService>();

        // Kafka
        services.AddSingleton<IKafkaProducer, KafkaProducer>();

        return services;
    }
}
```

### DI Rules:
- Constructor injection as default
- **Never** resolve scoped services from singletons
- Use `IOptions<T>` / `IOptionsMonitor<T>` for configuration
- Validate options early — fail fast on bad config
- Keep service registration in extension methods, not bloating `Program.cs`

## 6. Configuration & Secrets

### Configuration Sources (auto-loaded):
- `appsettings.json`
- `appsettings.{Environment}.json`
- Environment variables
- Command-line args

### Secrets:
- **Development:** Secret Manager (`dotnet user-secrets`)
- **Production:** Secure external store (Azure Key Vault, Docker secrets, etc.)
- **Never** commit secrets to source control

### Options Pattern:
```csharp
public class MongoDbSettings
{
    public string ConnectionString { get; set; } = default!;
    public string DatabaseName { get; set; } = default!;
}

public class JwtSettings
{
    public string Secret { get; set; } = default!;
    public string Issuer { get; set; } = default!;
    public string Audience { get; set; } = default!;
    public int ExpirationMinutes { get; set; } = 60;
    public int RefreshTokenExpirationDays { get; set; } = 7;
}

public class KafkaSettings
{
    public string BootstrapServers { get; set; } = default!;
    public string GroupId { get; set; } = default!;
}

public class RedisSettings
{
    public string ConnectionString { get; set; } = default!;
    public int DefaultExpirationSeconds { get; set; } = 300;
}
```

## 7. Error Handling

### Global Exception Middleware:
```csharp
public class ExceptionMiddleware
{
    private readonly RequestDelegate _next;
    private readonly ILogger<ExceptionMiddleware> _logger;

    public ExceptionMiddleware(RequestDelegate next, ILogger<ExceptionMiddleware> logger)
    {
        _next = next;
        _logger = logger;
    }

    public async Task InvokeAsync(HttpContext context)
    {
        try
        {
            await _next(context);
        }
        catch (ValidationException ex)
        {
            context.Response.StatusCode = StatusCodes.Status400BadRequest;
            await context.Response.WriteAsJsonAsync(new ProblemDetails
            {
                Status = 400,
                Title = "Validation Error",
                Detail = ex.Message,
                Extensions = { ["errors"] = ex.Errors }
            });
        }
        catch (NotFoundException ex)
        {
            context.Response.StatusCode = StatusCodes.Status404NotFound;
            await context.Response.WriteAsJsonAsync(new ProblemDetails
            {
                Status = 404,
                Title = "Not Found",
                Detail = ex.Message
            });
        }
        catch (UnauthorizedAccessException)
        {
            context.Response.StatusCode = StatusCodes.Status401Unauthorized;
            await context.Response.WriteAsJsonAsync(new ProblemDetails
            {
                Status = 401,
                Title = "Unauthorized"
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Unhandled exception");
            context.Response.StatusCode = StatusCodes.Status500InternalServerError;
            await context.Response.WriteAsJsonAsync(new ProblemDetails
            {
                Status = 500,
                Title = "Internal Server Error",
                Detail = context.Environment.IsDevelopment() ? ex.Message : "An error occurred"
            });
        }
    }
}
```

### Rules:
- Centralized exception handling via middleware — **not** scattered try/catch
- Always return `ProblemDetails` format
- Never leak internal exception details in production
- Custom exceptions: `NotFoundException`, `ValidationException`, `ForbiddenException`
- Developer exception page limited to Development environment only

## 8. Logging

```csharp
public class CreateTaskHandler : IRequestHandler<CreateTaskCommand, TaskDto>
{
    private readonly ILogger<CreateTaskHandler> _logger;

    public async Task<TaskDto> Handle(CreateTaskCommand request, CancellationToken ct)
    {
        _logger.LogInformation("Creating task {Title} for user {UserId}",
            request.Title, request.UserId);
        // ...
    }
}
```

### Rules:
- Use `ILogger<T>` from DI
- **Structured logging** — named parameters, not string concatenation
- Use Serilog as the sink (Console + File + optional Seq)
- Correlation IDs in middleware, not business logic
- Never log sensitive data (passwords, tokens, PII)

## 9. Authentication (JWT)

### Auth Middleware Stack:
```
Request
  ↓
Authentication (JWT Bearer validation)
  ↓
Rate Limit (Redis sliding window)
  ↓
Cache Middleware (Redis response cache)
  ↓
Controller
```

### JWT Setup:
```csharp
builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
        var jwtSettings = builder.Configuration
            .GetSection("Jwt").Get<JwtSettings>()!;

        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer = true,
            ValidateAudience = true,
            ValidateLifetime = true,
            ValidateIssuerSigningKey = true,
            ValidIssuer = jwtSettings.Issuer,
            ValidAudience = jwtSettings.Audience,
            IssuerSigningKey = new SymmetricSecurityKey(
                Encoding.UTF8.GetBytes(jwtSettings.Secret)),
            ClockSkew = TimeSpan.Zero
        };
    });
```

### Rules:
- `UseAuthentication()` **before** `UseAuthorization()`
- JWT + Refresh Token pattern
- Short-lived access tokens (15-60 min)
- Refresh tokens stored in MongoDB, rotated on use
- Password hashing: BCrypt
- `[Authorize]` at controller level, `[AllowAnonymous]` for login/register

## 10. Rate Limiting

### Built-in .NET 10 Rate Limiter:
```csharp
builder.Services.AddRateLimiter(options =>
{
    options.AddFixedWindowLimiter("authenticated", opt =>
    {
        opt.PermitLimit = 100;
        opt.Window = TimeSpan.FromMinutes(1);
        opt.QueueLimit = 0;
    });

    options.AddFixedWindowLimiter("anonymous", opt =>
    {
        opt.PermitLimit = 30;
        opt.Window = TimeSpan.FromMinutes(1);
        opt.QueueLimit = 0;
    });

    options.RejectionStatusCode = StatusCodes.Status429TooManyRequests;
});
```

### Custom Redis Rate Limiter (for distributed):
- Sliding window algorithm in Redis
- Per-user key: `ratelimit:{userId}:{endpoint}`
- Per-IP key: `ratelimit:{ip}:{endpoint}`
- Return `429 Too Many Requests` with `Retry-After` header

## 11. Caching (Redis)

```csharp
public interface ICacheService
{
    Task<T?> GetAsync<T>(string key, CancellationToken ct = default);
    Task SetAsync<T>(string key, T value, TimeSpan? expiration = null, CancellationToken ct = default);
    Task RemoveAsync(string key, CancellationToken ct = default);
    Task RemoveByPrefixAsync(string prefix, CancellationToken ct = default);
}
```

### Cache Strategy:
- **GET endpoints:** 30s cache
- **Cache key:** `{service}:{userId}:{endpoint}:{queryHash}`
- **Invalidation:** On write operations (Create/Update/Delete)
- **Pattern invalidation:** `RemoveByPrefixAsync("task:{userId}:*")`
- Keep cached data derivable from durable source (MongoDB)
- Separate cache shape from persistence shape

## 12. Kafka Event-Driven Communication

### Events Published:
```csharp
public record TaskCreatedEvent(string TaskId, string Title, string UserId, DateTime CreatedAt);
public record TaskCompletedEvent(string TaskId, string UserId, DateTime CompletedAt);
public record TaskDueApproachingEvent(string TaskId, string Title, string UserId, DateTime DueDate);
public record ProjectUpdatedEvent(string ProjectId, string Name, string UpdatedBy);
```

### Producer Pattern:
```csharp
public interface IKafkaProducer
{
    Task PublishAsync<T>(string topic, string key, T message, CancellationToken ct = default);
}
```

### Topics:
| Topic | Producer | Consumer |
|-------|----------|----------|
| `task-events` | Task Service | Notification Service |
| `project-events` | Project Service | Notification Service |
| `user-events` | Auth Service | Task Service, Project Service |

### Consumer: BackgroundService
```csharp
public class TaskEventConsumer : BackgroundService
{
    // Use IHostedService/BackgroundService for Kafka consumers
    // Create scopes for scoped dependencies
    // Respect cancellation tokens
    // Keep observable and small
}
```

## 13. SignalR (Real-time Notifications)

```csharp
[Authorize]
public class NotificationHub : Hub
{
    // Hub = communication boundary, NOT business logic
    public override async Task OnConnectedAsync()
    {
        var userId = Context.User!.FindFirst(ClaimTypes.NameIdentifier)!.Value;
        await Groups.AddToGroupAsync(Context.ConnectionId, $"user-{userId}");
        await base.OnConnectedAsync();
    }
}
```

### Rules:
- Hub is a thin communication layer
- Authenticate connections (JWT)
- Use groups for user targeting
- Plan for scale-out (Redis backplane) in multi-instance

## 14. Health Checks

```csharp
builder.Services.AddHealthChecks()
    .AddMongoDb(mongoConnectionString, name: "mongodb")
    .AddRedis(redisConnectionString, name: "redis")
    .AddKafka(kafkaConfig, name: "kafka");

app.MapHealthChecks("/health", new HealthCheckOptions
{
    ResponseWriter = UIResponseWriter.WriteHealthCheckUIResponse
});

app.MapHealthChecks("/health/ready", new HealthCheckOptions
{
    Predicate = check => check.Tags.Contains("ready")
});

app.MapHealthChecks("/health/live", new HealthCheckOptions
{
    Predicate = _ => false // Just checks if the app is running
});
```

### Every service MUST expose:
- `/health` — full dependency check
- `/health/ready` — readiness probe
- `/health/live` — liveness probe

## 15. API Versioning

```csharp
// URL-based versioning: /api/v1/tasks
[ApiController]
[Route("api/v1/[controller]")]
public class TasksController : ControllerBase { }
```

- URL-based: `/api/v1/...`
- Keep route + payload contracts stable per version
- Breaking changes → new version

## 16. Testing Strategy

### Layered Testing:
| Layer | Tool | Scope |
|-------|------|-------|
| Unit | xUnit + Moq | Handlers, Services, Validators |
| Integration | `WebApplicationFactory<Program>` | Pipeline, DI, DB, Auth |
| Browser | Playwright | E2E user flows |

### Integration Test Pattern:
```csharp
public class TasksControllerTests : IClassFixture<WebApplicationFactory<Program>>
{
    private readonly HttpClient _client;

    public TasksControllerTests(WebApplicationFactory<Program> factory)
    {
        _client = factory.WithWebHostBuilder(builder =>
        {
            builder.ConfigureServices(services =>
            {
                // Replace MongoDB with test container or in-memory
            });
        }).CreateClient();
    }
}
```

## 17. Project Structure (Solution)

```
TaskManager.sln
├── src/
│   ├── Gateway/
│   │   └── TaskManager.Gateway/              # Ocelot API Gateway
│   ├── Services/
│   │   ├── Auth/
│   │   │   ├── TaskManager.Auth.Domain/
│   │   │   ├── TaskManager.Auth.Application/
│   │   │   ├── TaskManager.Auth.Infrastructure/
│   │   │   └── TaskManager.Auth.API/
│   │   ├── Task/
│   │   │   ├── TaskManager.Task.Domain/
│   │   │   ├── TaskManager.Task.Application/
│   │   │   ├── TaskManager.Task.Infrastructure/
│   │   │   └── TaskManager.Task.API/
│   │   ├── Project/
│   │   │   ├── TaskManager.Project.Domain/
│   │   │   ├── TaskManager.Project.Application/
│   │   │   ├── TaskManager.Project.Infrastructure/
│   │   │   └── TaskManager.Project.API/
│   │   └── Notification/
│   │       ├── TaskManager.Notification.Domain/
│   │       ├── TaskManager.Notification.Application/
│   │       ├── TaskManager.Notification.Infrastructure/
│   │       └── TaskManager.Notification.API/
│   ├── Shared/
│   │   ├── TaskManager.Shared.Domain/        # Base entities, common value objects
│   │   ├── TaskManager.Shared.Application/   # Common DTOs, interfaces
│   │   └── TaskManager.Shared.Infrastructure/ # Common middleware, Kafka, Redis, Mongo helpers
│   └── Frontend/
│       └── task-manager-ui/                  # React app
├── tests/
│   ├── TaskManager.Auth.UnitTests/
│   ├── TaskManager.Auth.IntegrationTests/
│   ├── TaskManager.Task.UnitTests/
│   ├── TaskManager.Task.IntegrationTests/
│   └── ...
├── docker-compose.yml
├── docker-compose.override.yml
└── README.md
```

## 18. NuGet Packages (Per Service)

### Shared across all services:
- `MediatR` — CQRS
- `FluentValidation.AspNetCore` — Validation
- `Serilog.AspNetCore` — Structured logging
- `MongoDB.Driver` — Data access
- `StackExchange.Redis` — Cache
- `Confluent.Kafka` — Messaging
- `AspNetCore.HealthChecks.MongoDb`
- `AspNetCore.HealthChecks.Redis`
- `Swashbuckle.AspNetCore` — OpenAPI/Swagger

### Auth Service specific:
- `BCrypt.Net-Next` — Password hashing
- `System.IdentityModel.Tokens.Jwt` — JWT

### Gateway specific:
- `Ocelot` — API Gateway

### Notification Service specific:
- `Microsoft.AspNetCore.SignalR` — Real-time

## 19. Anti-Patterns (Flag During Review)

- [ ] Business logic in controllers (should be in handlers)
- [ ] `Startup` class (use modern `WebApplication`)
- [ ] Manual `new HttpClient()` (use `IHttpClientFactory`)
- [ ] String concatenation in logs (use structured logging)
- [ ] Secrets in `appsettings.json` (use Secret Manager / env vars)
- [ ] Resolving scoped from singleton
- [ ] `try/catch` in controllers (use exception middleware)
- [ ] Ad hoc error JSON (use `ProblemDetails`)
- [ ] `transition: all` in middleware order (wrong sequence)
- [ ] Missing health checks
- [ ] Entities exposed in API contracts (use DTOs)
- [ ] `UseAuthentication()` after `UseAuthorization()`
- [ ] Missing `[ApiController]` attribute
- [ ] `Controller` base class instead of `ControllerBase` for APIs

---

*All backend code will be reviewed against these guidelines + official ASP.NET Core documentation.*
