# Hướng Dẫn Thực Hành: Building Task Manager Service (Chi Tiết từng Bước)

## Giới Thiệu

Tài liệu này hướng dẫn bạn **xây dựng Task Service từ đầu** - bao gồm tất cả code, config, và giải thích chi tiết.

Sau khi hiểu xong, bạn có thể dùng điều này làm template để tạo các service khác (Project, Notification, v.v).

---

## Phần 1: Setup Project Structure

### Bước 1.1: Tạo Folder & Projects

```bash
# Mở terminal từ c:\Users\tuand\OneDrive\Documents\GitHub\Task

# Tạo folder cho service
mkdir src\Services\Task

# Tạo 4 project (.csproj files)
cd src\Services\Task
dotnet new classlib -n TaskManager.Task.Domain
dotnet new classlib -n TaskManager.Task.Application
dotnet new classlib -n TaskManager.Task.Infrastructure
dotnet new webapi -n TaskManager.Task.API

# Back to root
cd ..\..\..\
```

**Kết quả:**
```
src/Services/Task/
├── TaskManager.Task.Domain/
│   └── TaskManager.Task.Domain.csproj
├── TaskManager.Task.Application/
│   └── TaskManager.Task.Application.csproj
├── TaskManager.Task.Infrastructure/
│   └── TaskManager.Task.Infrastructure.csproj
└── TaskManager.Task.API/
    └── TaskManager.Task.API.csproj
```

### Bước 1.2: Thêm Project References

Mục đích: Establish layer dependencies
```
API → Application → Domain
   → Infrastructure → Domain
```

**Cách 1: Dùng Visual Studio** (Project Properties → Add Reference)

**Cách 2: Edit .csproj files**

[TaskManager.Task.Application.csproj](../src/Services/Task/TaskManager.Task.Application/TaskManager.Task.Application.csproj) TẠO TỰ ĐỘNG.

```xml
<ItemGroup>
  <ProjectReference Include="..\..\..\Shared\TaskManager.Shared.Application\TaskManager.Shared.Application.csproj" />
  <ProjectReference Include="..\TaskManager.Task.Domain\TaskManager.Task.Domain.csproj" />
</ItemGroup>
```

### Bước 1.3: Thêm NuGet Dependencies

```bash
# Application Layer
cd src/Services/Task/TaskManager.Task.Application
dotnet add package MediatR
dotnet add package FluentValidation

# Infrastructure Layer
cd ../TaskManager.Task.Infrastructure
dotnet add package MongoDB.Driver
dotnet add package StackExchange.Redis

# API Layer
cd ../TaskManager.Task.API
dotnet add package Serilog.AspNetCore
dotnet add package Serilog.Sinks.Console

cd ..\..\..\..\
```

---

## Phần 2: Xây Dựng Domain Layer

### Bước 2.1: Tạo Enums

**File:** `src/Services/Task/TaskManager.Task.Domain/Enums/TaskItemStatus.cs`

```csharp
namespace TaskManager.Task.Domain.Enums;

public enum TaskItemStatus
{
    Todo = 0,
    InProgress = 1,
    Review = 2,
    Done = 3
}
```

**File:** `src/Services/Task/TaskManager.Task.Domain/Enums/Priority.cs`

```csharp
namespace TaskManager.Task.Domain.Enums;

public enum Priority
{
    Low = 0,
    Medium = 1,
    High = 2,
    Urgent = 3
}
```

### Bước 2.2: Tạo Comment Entity (Nested Object)

**File:** `src/Services/Task/TaskManager.Task.Domain/Entities/TaskComment.cs`

```csharp
using MongoDB.Bson;

namespace TaskManager.Task.Domain.Entities;

public class TaskComment
{
    // Mỗi comment cũng cần một ID (trong document của task)
    public string Id { get; set; } = ObjectId.GenerateNewId().ToString();
    
    public string UserId { get; set; } = default!;
    public string Content { get; set; } = default!;
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
}
```

### Bước 2.3: Tạo TaskItem Entity (Main Entity)

**File:** `src/Services/Task/TaskManager.Task.Domain/Entities/TaskItem.cs`

```csharp
using TaskManager.Shared.Domain.Entities;
using TaskManager.Task.Domain.Enums;

namespace TaskManager.Task.Domain.Entities;

public class TaskItem : AuditableEntity
{
    // Basic info
    public string Title { get; set; } = default!;
    public string Description { get; set; } = default!;
    
    // Relations
    public string ProjectId { get; set; } = default!;
    public string AssigneeId { get; set; } = default!;
    public string ReporterId { get; set; } = default!;
    
    // Status
    public TaskItemStatus Status { get; set; } = TaskItemStatus.Todo;
    public Priority Priority { get; set; } = Priority.Medium;
    public DateTime? DueDate { get; set; }
    
    // Collections
    public List<string> Tags { get; set; } = [];
    public List<TaskComment> Comments { get; set; } = [];
}
```

**Giải Thích:**
- Kế thừa từ `AuditableEntity` → Tự động có `Id`, `CreatedAt`, `UpdatedAt`
- `ProjectId`, `AssigneeId`, `ReporterId` → IDs từ các services khác (Microservices)
- `Tags` → List<string> cho flexible tagging
- `Comments` → List<TaskComment> để lưu comments nested (MongoDB strengths)

### Bước 2.4: Tạo Repository Interface

**File:** `src/Services/Task/TaskManager.Task.Domain/Interfaces/ITaskItemRepository.cs`

```csharp
using TaskManager.Task.Domain.Entities;
using TaskManager.Task.Domain.Enums;
using TaskManager.Shared.Application.Interfaces;

namespace TaskManager.Task.Domain.Interfaces;

public interface ITaskItemRepository : IRepository<TaskItem>
{
    // Extend base repository với service-specific queries
    
    Task<List<TaskItem>> GetByProjectIdAsync(
        string projectId,
        CancellationToken ct = default);
    
    Task<List<TaskItem>> GetByAssigneeIdAsync(
        string assigneeId,
        CancellationToken ct = default);
    
    Task<List<TaskItem>> GetByStatusAsync(
        TaskItemStatus status,
        CancellationToken ct = default);
    
    Task<List<TaskItem>> GetByReporterIdAsync(
        string reporterId,
        CancellationToken ct = default);
}
```

**Domain Layer hoàn tất!** ✓

---

## Phần 3: Xây Dựng Application Layer

### Bước 3.1: Tạo DTOs (Data Transfer Objects)

**File:** `src/Services/Task/TaskManager.Task.Application/DTOs/TaskResponse.cs`

```csharp
namespace TaskManager.Task.Application.DTOs;

public record TaskResponse(
    string Id,
    string Title,
    string Description,
    string ProjectId,
    string AssigneeId,
    string ReporterId,
    string Status,  // enum as string in API
    string Priority,
    DateTime? DueDate,
    List<string> Tags,
    DateTime CreatedAt,
    DateTime UpdatedAt);
```

**File:** `src/Services/Task/TaskManager.Task.Application/DTOs/CreateTaskRequest.cs`

```csharp
namespace TaskManager.Task.Application.DTOs;

public record CreateTaskRequest(
    string Title,
    string Description,
    string ProjectId,
    string AssigneeId,
    string? Status = null,
    string? Priority = null,
    DateTime? DueDate = null,
    List<string>? Tags = null);
```

**File:** `src/Services/Task/TaskManager.Task.Application/DTOs/UpdateTaskRequest.cs`

```csharp
namespace TaskManager.Task.Application.DTOs;

public record UpdateTaskRequest(
    string Title,
    string Description,
    string? Status = null,
    string? Priority = null,
    DateTime? DueDate = null,
    List<string>? Tags = null);
```

### Bước 3.2: Tạo Create Command & Handler

**File:** `src/Services/Task/TaskManager.Task.Application/Commands/CreateTask/CreateTaskCommand.cs`

```csharp
using MediatR;
using TaskManager.Task.Application.DTOs;

namespace TaskManager.Task.Application.Commands.CreateTask;

public record CreateTaskCommand(
    string Title,
    string Description,
    string ProjectId,
    string AssigneeId,
    string? Status = null,
    string? Priority = null,
    DateTime? DueDate = null,
    List<string>? Tags = null) : IRequest<TaskResponse>;
```

**File:** `src/Services/Task/TaskManager.Task.Application/Commands/CreateTask/CreateTaskCommandValidator.cs`

```csharp
using FluentValidation;

namespace TaskManager.Task.Application.Commands.CreateTask;

public class CreateTaskCommandValidator : AbstractValidator<CreateTaskCommand>
{
    public CreateTaskCommandValidator()
    {
        RuleFor(x => x.Title)
            .NotEmpty()
            .WithMessage("Title is required")
            .MaximumLength(200)
            .WithMessage("Title must not exceed 200 characters");

        RuleFor(x => x.Description)
            .MaximumLength(2000)
            .WithMessage("Description must not exceed 2000 characters");

        RuleFor(x => x.ProjectId)
            .NotEmpty()
            .WithMessage("ProjectId is required");

        RuleFor(x => x.AssigneeId)
            .NotEmpty()
            .WithMessage("AssigneeId is required");

        RuleFor(x => x.DueDate)
            .GreaterThan(DateTime.UtcNow)
            .When(x => x.DueDate.HasValue)
            .WithMessage("DueDate must be in the future");
    }
}
```

**File:** `src/Services/Task/TaskManager.Task.Application/Commands/CreateTask/CreateTaskCommandHandler.cs`

```csharp
using MediatR;
using TaskManager.Task.Domain.Entities;
using TaskManager.Task.Domain.Enums;
using TaskManager.Task.Domain.Interfaces;
using TaskManager.Task.Application.DTOs;
using TaskManager.Shared.Infrastructure.Messaging;
using System.Security.Claims;
using Microsoft.AspNetCore.Http;

namespace TaskManager.Task.Application.Commands.CreateTask;

public class CreateTaskCommandHandler : IRequestHandler<CreateTaskCommand, TaskResponse>
{
    private readonly ITaskItemRepository _repository;
    private readonly IKafkaProducer _kafkaProducer;
    private readonly IHttpContextAccessor _httpContextAccessor;

    public CreateTaskCommandHandler(
        ITaskItemRepository repository,
        IKafkaProducer kafkaProducer,
        IHttpContextAccessor httpContextAccessor)
    {
        _repository = repository;
        _kafkaProducer = kafkaProducer;
        _httpContextAccessor = httpContextAccessor;
    }

    public async Task<TaskResponse> Handle(
        CreateTaskCommand request,
        CancellationToken cancellationToken)
    {
        // 1. Get current user from claims
        var userId = _httpContextAccessor.HttpContext!.User
            .FindFirst(ClaimTypes.NameIdentifier)?.Value
            ?? throw new UnauthorizedAccessException("User not authenticated");

        // 2. Parse enum values
        var status = Enum.TryParse<TaskItemStatus>(request.Status ?? "Todo", out var s) 
            ? s 
            : TaskItemStatus.Todo;
        
        var priority = Enum.TryParse<Priority>(request.Priority ?? "Medium", out var p) 
            ? p 
            : Priority.Medium;

        // 3. Create entity
        var taskItem = new TaskItem
        {
            Title = request.Title,
            Description = request.Description,
            ProjectId = request.ProjectId,
            AssigneeId = request.AssigneeId,
            ReporterId = userId,
            Status = status,
            Priority = priority,
            DueDate = request.DueDate,
            Tags = request.Tags ?? []
        };

        // 4. Save to database
        var result = await _repository.AddAsync(taskItem, cancellationToken);

        // 5. Publish Kafka event (for Notification Service to subscribe)
        await _kafkaProducer.ProduceAsync(
            "task.created",
            new
            {
                id = result.Id,
                title = result.Title,
                assigneeId = result.AssigneeId,
                reporterId = result.ReporterId
            },
            cancellationToken);

        // 6. Return DTO
        return MapToResponse(result);
    }

    private static TaskResponse MapToResponse(TaskItem task)
    {
        return new TaskResponse(
            task.Id,
            task.Title,
            task.Description,
            task.ProjectId,
            task.AssigneeId,
            task.ReporterId,
            task.Status.ToString(),
            task.Priority.ToString(),
            task.DueDate,
            task.Tags,
            task.CreatedAt,
            task.UpdatedAt);
    }
}
```

### Bước 3.3: Tạo Get Query & Handler

**File:** `src/Services/Task/TaskManager.Task.Application/Queries/GetTaskById/GetTaskByIdQuery.cs`

```csharp
using MediatR;
using TaskManager.Task.Application.DTOs;

namespace TaskManager.Task.Application.Queries.GetTaskById;

public record GetTaskByIdQuery(string TaskId) : IRequest<TaskResponse>;
```

**File:** `src/Services/Task/TaskManager.Task.Application/Queries/GetTaskById/GetTaskByIdQueryHandler.cs`

```csharp
using MediatR;
using TaskManager.Task.Domain.Interfaces;
using TaskManager.Task.Application.DTOs;
using TaskManager.Shared.Application.Interfaces;

namespace TaskManager.Task.Application.Queries.GetTaskById;

public class GetTaskByIdQueryHandler : IRequestHandler<GetTaskByIdQuery, TaskResponse>
{
    private readonly ITaskItemRepository _repository;
    private readonly ICacheService _cacheService;

    public GetTaskByIdQueryHandler(
        ITaskItemRepository repository,
        ICacheService cacheService)
    {
        _repository = repository;
        _cacheService = cacheService;
    }

    public async Task<TaskResponse> Handle(
        GetTaskByIdQuery request,
        CancellationToken cancellationToken)
    {
        var cacheKey = $"task:{request.TaskId}";

        // 1. Try get from cache
        var cached = await _cacheService.GetAsync<TaskResponse>(cacheKey, cancellationToken);
        if (cached != null)
            return cached;

        // 2. Get from database
        var task = await _repository.GetByIdAsync(request.TaskId, cancellationToken)
            ?? throw new KeyNotFoundException($"Task with ID '{request.TaskId}' not found");

        // 3. Map to DTO
        var response = new TaskResponse(
            task.Id,
            task.Title,
            task.Description,
            task.ProjectId,
            task.AssigneeId,
            task.ReporterId,
            task.Status.ToString(),
            task.Priority.ToString(),
            task.DueDate,
            task.Tags,
            task.CreatedAt,
            task.UpdatedAt);

        // 4. Cache for 1 hour
        await _cacheService.SetAsync(cacheKey, response, TimeSpan.FromHours(1), cancellationToken);

        return response;
    }
}
```

**Application Layer hoàn tất!** ✓

---

## Phần 4: Xây Dựng Infrastructure Layer

### Bước 4.1: Implement MongoDB Repository

**File:** `src/Services/Task/TaskManager.Task.Infrastructure/Persistence/TaskItemRepository.cs`

```csharp
using MongoDB.Driver;
using TaskManager.Task.Domain.Entities;
using TaskManager.Task.Domain.Enums;
using TaskManager.Task.Domain.Interfaces;
using TaskManager.Shared.Infrastructure.Persistence;

namespace TaskManager.Task.Infrastructure.Persistence;

public class TaskItemRepository : MongoRepository<TaskItem>, ITaskItemRepository
{
    public TaskItemRepository(IMongoDatabase database)
        : base(database, "tasks")
    {
    }

    public async Task<List<TaskItem>> GetByProjectIdAsync(
        string projectId,
        CancellationToken ct = default)
    {
        var filter = Builders<TaskItem>.Filter.Eq(x => x.ProjectId, projectId);
        return await _collection
            .Find(filter)
            .SortByDescending(x => x.CreatedAt)
            .ToListAsync(ct);
    }

    public async Task<List<TaskItem>> GetByAssigneeIdAsync(
        string assigneeId,
        CancellationToken ct = default)
    {
        var filter = Builders<TaskItem>.Filter.Eq(x => x.AssigneeId, assigneeId);
        return await _collection
            .Find(filter)
            .SortByDescending(x => x.DueDate)
            .ToListAsync(ct);
    }

    public async Task<List<TaskItem>> GetByStatusAsync(
        TaskItemStatus status,
        CancellationToken ct = default)
    {
        var filter = Builders<TaskItem>.Filter.Eq(x => x.Status, status);
        return await _collection
            .Find(filter)
            .SortByDescending(x => x.CreatedAt)
            .ToListAsync(ct);
    }

    public async Task<List<TaskItem>> GetByReporterIdAsync(
        string reporterId,
        CancellationToken ct = default)
    {
        var filter = Builders<TaskItem>.Filter.Eq(x => x.ReporterId, reporterId);
        return await _collection
            .Find(filter)
            .SortByDescending(x => x.CreatedAt)
            .ToListAsync(ct);
    }
}
```

### Bước 4.2: Tạo Dependency Injection File

**File:** `src/Services/Task/TaskManager.Task.Infrastructure/DependencyInjection.cs`

```csharp
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using TaskManager.Task.Domain.Interfaces;
using TaskManager.Task.Infrastructure.Persistence;
using TaskManager.Shared.Infrastructure.Extensions;

namespace TaskManager.Task.Infrastructure;

public static class DependencyInjection
{
    public static IServiceCollection AddTaskInfrastructure(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        // MongoDB
        services.AddMongoDb(configuration);

        // Redis Cache
        services.AddRedisCache(configuration);

        // JWT Authentication
        services.AddJwtAuthentication(configuration);

        // Kafka Producer
        services.AddKafkaProducer(configuration);

        // Repositories
        services.AddScoped<ITaskItemRepository, TaskItemRepository>();

        return services;
    }
}
```

**Infrastructure Layer hoàn tất!** ✓

---

## Phần 5: Xây Dựng API Layer

### Bước 5.1: Tạo Controller

**File:** `src/Services/Task/TaskManager.Task.API/Controllers/TasksController.cs`

```csharp
using System.Security.Claims;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using TaskManager.Task.Application.Commands.CreateTask;
using TaskManager.Task.Application.DTOs;
using TaskManager.Task.Application.Queries.GetTaskById;

namespace TaskManager.Task.API.Controllers;

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

    // POST /api/v1/tasks
    [HttpPost]
    [ProducesResponseType(typeof(TaskResponse), StatusCodes.Status201Created)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> CreateTask([FromBody] CreateTaskRequest request)
    {
        var command = new CreateTaskCommand(
            request.Title,
            request.Description,
            request.ProjectId,
            request.AssigneeId,
            request.Status,
            request.Priority,
            request.DueDate,
            request.Tags);

        var result = await _mediator.Send(command);
        return CreatedAtAction(nameof(GetTask), new { id = result.Id }, result);
    }

    // GET /api/v1/tasks/{id}
    [HttpGet("{id}")]
    [ProducesResponseType(typeof(TaskResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetTask(string id)
    {
        var result = await _mediator.Send(new GetTaskByIdQuery(id));
        return Ok(result);
    }

    // GET /api/v1/tasks (get my tasks)
    [HttpGet]
    public async Task<IActionResult> GetMyTasks()
    {
        var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        // TODO: Implement GetMyTasksQuery
        return Ok(new { message = "Not yet implemented" });
    }
}
```

### Bước 5.2: Setup Program.cs

**File:** `src/Services/Task/TaskManager.Task.API/Program.cs`

```csharp
using Serilog;
using TaskManager.Task.Infrastructure;
using TaskManager.Shared.Infrastructure.Extensions;

var builder = WebApplication.CreateBuilder(args);

// ─── Serilog Logging ────────────────────────────────────
builder.Host.UseSerilog((context, config) =>
{
    config
        .ReadFrom.Configuration(context.Configuration)
        .WriteTo.Console()
        .Enrich.FromLogContext();
});

// ─── Services ───────────────────────────────────────────

// Application services (MediatR + FluentValidation)
builder.Services.AddApplicationServices(
    typeof(TaskManager.Task.Application.Commands.CreateTask.CreateTaskCommand).Assembly);

// Infrastructure services (MongoDB, Redis, Kafka, JWT)
builder.Services.AddTaskInfrastructure(builder.Configuration);

// API services
builder.Services.AddControllers();
builder.Services.AddOpenApi();
builder.Services.AddHealthChecks();

// Add HttpContextAccessor for CurrentUser
builder.Services.AddHttpContextAccessor();

var app = builder.Build();

// ─── Middleware Pipeline (ORDER MATTERS!) ────────────────

if (!app.Environment.IsDevelopment())
{
    app.UseHsts();
}

app.UseHttpsRedirection();

// Custom shared middleware (exception handling)
app.UseSharedMiddleware();

// Authentication & Authorization
app.UseAuthentication();
app.UseAuthorization();

// Map endpoints
app.MapControllers();
app.MapHealthChecks("/health");

// OpenAPI in development
if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
}

app.Run();

// Make Program accessible for integration tests
public partial class Program { }
```

### Bước 5.3: Setup appsettings.json

**File:** `src/Services/Task/TaskManager.Task.API/appsettings.json`

```json
{
  "Serilog": {
    "MinimumLevel": {
      "Default": "Information",
      "Override": {
        "Microsoft": "Warning",
        "System": "Warning"
      }
    },
    "WriteTo": [
      {
        "Name": "Console",
        "Args": {
          "theme": "Ansi"
        }
      }
    ]
  },
  "MongoDB": {
    "ConnectionString": "mongodb://localhost:27017",
    "DatabaseName": "task_db"
  },
  "Redis": {
    "ConnectionString": "localhost:6379"
  },
  "Jwt": {
    "Secret": "your-super-secret-key-minimum-32-characters-very-secure!!!",
    "Issuer": "task-manager",
    "Audience": "task-manager-api",
    "ExpirationMinutes": 60
  },
  "Kafka": {
    "BootstrapServers": "localhost:9092"
  }
}
```

### Bước 5.4: Setup launchSettings.json

**File:** `src/Services/Task/TaskManager.Task.API/Properties/launchSettings.json`

```json
{
  "$schema": "https://json.schemastore.org/launchsettings.json",
  "iisSettings": {
    "windowsAuthentication": false,
    "anonymousAuthentication": true
  },
  "profiles": {
    "Development": {
      "commandName": "Project",
      "environmentVariables": {
        "ASPNETCORE_ENVIRONMENT": "Development"
      },
      "applicationUrl": "https://localhost:5002;http://localhost:5002"
    },
    "Production": {
      "commandName": "Project",
      "environmentVariables": {
        "ASPNETCORE_ENVIRONMENT": "Production"
      },
      "applicationUrl": "https://localhost:5002"
    }
  }
}
```

**API Layer hoàn tất!** ✓

---

## Phần 6: Cập Nhật Gateway

### Bước 6.1: Cập Nhật Ocelot Config

**File:** `src/Gateway/TaskManager.Gateway/ocelot.json`

```json
{
  "Routes": [
    {
      "DownstreamPathTemplate": "/api/v1/{everything}",
      "DownstreamScheme": "https",
      "DownstreamHostAndPorts": [
        {
          "Host": "localhost",
          "Port": 5001
        }
      ],
      "UpstreamPathTemplate": "/api/auth/{everything}",
      "UpstreamHttpMethod": [ "Get", "Post", "Put", "Delete" ]
    },
    {
      "DownstreamPathTemplate": "/api/v1/{everything}",
      "DownstreamScheme": "https",
      "DownstreamHostAndPorts": [
        {
          "Host": "localhost",
          "Port": 5002
        }
      ],
      "UpstreamPathTemplate": "/api/tasks/{everything}",
      "UpstreamHttpMethod": [ "Get", "Post", "Put", "Delete" ]
    },
    {
      "DownstreamPathTemplate": "/api/v1/{everything}",
      "DownstreamScheme": "https",
      "DownstreamHostAndPorts": [
        {
          "Host": "localhost",
          "Port": 5003
        }
      ],
      "UpstreamPathTemplate": "/api/projects/{everything}",
      "UpstreamHttpMethod": [ "Get", "Post", "Put", "Delete" ]
    },
    {
      "DownstreamPathTemplate": "/api/v1/{everything}",
      "DownstreamScheme": "https",
      "DownstreamHostAndPorts": [
        {
          "Host": "localhost",
          "Port": 5004
        }
      ],
      "UpstreamPathTemplate": "/api/notifications/{everything}",
      "UpstreamHttpMethod": [ "Get", "Post", "Put", "Delete" ]
    }
  ],
  "GlobalConfiguration": {
    "BaseUrl": "https://localhost:5000"
  }
}
```

---

## Phần 7: Kiểm Tra & Troubleshoot

### Bước 7.1: Build Project

```bash
cd c:\Users\tuand\OneDrive\Documents\GitHub\Task
dotnet build TaskManager.slnx
```

**Nếu có errors:**

| Error | Giải Pháp |
|-------|----------|
| **Project reference not found** | Kiểm tra .csproj files, paths phải chính xác |
| **NuGet package not installed** | Run `dotnet restore` hoặc thêm package lại |
| **Using statement errors** | Kiểm tra namespace imports |
| **Missing base class** | Đảm bảo Shared projects có base entities |

### Bước 7.2: Test API Endpoints

```bash
# 1. Chạy services (mỗi terminal riêng)
cd src/Services/Auth/TaskManager.Auth.API
dotnet run

cd src/Services/Task/TaskManager.Task.API
dotnet run

# 2. Register user (Auth Service)
curl -X POST https://localhost:5001/api/v1/auth/register \
  -H "Content-Type: application/json" \
  -d '{
    "email": "user@example.com",
    "password": "password123",
    "displayName": "User Name"
  }'

# 3. Get JWT token (copy từ response)
# 4. Create task (Task Service)
curl -X POST https://localhost:5002/api/v1/tasks \
  -H "Content-Type: application/json" \
  -H "Authorization: Bearer <YOUR_JWT_TOKEN>" \
  -d '{
    "title": "My First Task",
    "description": "Task description",
    "projectId": "proj123",
    "assigneeId": "user123"
  }'

# 5. Get task
curl -X GET https://localhost:5002/api/v1/tasks/{taskId} \
  -H "Authorization: Bearer <YOUR_JWT_TOKEN>"
```

---

## Phần 8: Mở Rộng - Thêm Update & Delete

### Bước 8.1: Update Command

```csharp
// Commands/UpdateTask/UpdateTaskCommand.cs
public record UpdateTaskCommand(
    string Id,
    string Title,
    string Description,
    string? Status = null,
    string? Priority = null,
    DateTime? DueDate = null,
    List<string>? Tags = null) : IRequest<TaskResponse>;

// Commands/UpdateTask/UpdateTaskCommandHandler.cs
public class UpdateTaskCommandHandler : IRequestHandler<UpdateTaskCommand, TaskResponse>
{
    private readonly ITaskItemRepository _repository;
    private readonly ICacheService _cacheService;
    private readonly IKafkaProducer _kafkaProducer;

    public async Task<TaskResponse> Handle(
        UpdateTaskCommand request,
        CancellationToken cancellationToken)
    {
        var task = await _repository.GetByIdAsync(request.Id, cancellationToken)
            ?? throw new KeyNotFoundException($"Task {request.Id} not found");

        // Update properties
        task.Title = request.Title;
        task.Description = request.Description;
        
        if (!string.IsNullOrEmpty(request.Status) &&
            Enum.TryParse<TaskItemStatus>(request.Status, out var status))
            task.Status = status;

        if (!string.IsNullOrEmpty(request.Priority) &&
            Enum.TryParse<Priority>(request.Priority, out var priority))
            task.Priority = priority;

        if (request.DueDate.HasValue)
            task.DueDate = request.DueDate;

        if (request.Tags != null)
            task.Tags = request.Tags;

        task.UpdatedAt = DateTime.UtcNow;

        // Save
        var result = await _repository.UpdateAsync(task, cancellationToken);

        // Invalidate cache
        await _cacheService.RemoveAsync($"task:{request.Id}", cancellationToken);

        // Publish event
        await _kafkaProducer.ProduceAsync(
            "task.updated",
            new { id = result.Id, status = result.Status.ToString() },
            cancellationToken);

        return MapToResponse(result);
    }
}
```

### Bước 8.2: Delete Command

```csharp
// Commands/DeleteTask/DeleteTaskCommand.cs
public record DeleteTaskCommand(string Id) : IRequest<Unit>;

// Commands/DeleteTask/DeleteTaskCommandHandler.cs
public class DeleteTaskCommandHandler : IRequestHandler<DeleteTaskCommand, Unit>
{
    private readonly ITaskItemRepository _repository;
    private readonly ICacheService _cacheService;
    private readonly IKafkaProducer _kafkaProducer;

    public async Task<Unit> Handle(
        DeleteTaskCommand request,
        CancellationToken cancellationToken)
    {
        var task = await _repository.GetByIdAsync(request.Id, cancellationToken)
            ?? throw new KeyNotFoundException($"Task {request.Id} not found");

        // Delete
        await _repository.DeleteAsync(request.Id, cancellationToken);

        // Invalidate cache
        await _cacheService.RemoveAsync($"task:{request.Id}", cancellationToken);

        // Publish event
        await _kafkaProducer.ProduceAsync(
            "task.deleted",
            new { id = request.Id },
            cancellationToken);

        return Unit.Value;
    }
}
```

---

## Phần 9: Common Pitfalls & Solutions

| Vấn Đề | Nguyên Nhân | Giải Pháp |
|--------|-----------|----------|
| **Task creation fails with "ProjectId not found"** | Project Service chưa implement, hoặc Task Service không call Project validation | Validate ProjectId tồn tại trước khi tạo task (gọi Project Service qua HTTP hoặc Kafka message) |
| **JWT token invalid** | Secret key khác nhau giữa Auth & Task Service | Đảm bảo `Jwt:Secret` giống nhau trong appsettings |
| **MongoDB connection fails** | Connection string sai hoặc MongoDB không chạy | Check `mongodb://localhost:27017` trong appsettings |
| **Cache not working** | Redis không chạy | Start Redis: `redis-server` hoặc check connection string |
| **Kafka message not consumed** | Consumer chưa implement hoặc topic sai | Implement NotificationConsumer, kiểm tra topic name match |
| **CORS error from Frontend** | Gateway không có CORS config | Add CORS middleware trong Gateway Program.cs |

---

## Phần 10: Summary - Checklist

**Domain Layer:**
- [ ] TaskItem.cs entity
- [ ] TaskComment.cs nested object
- [ ] Enums (TaskItemStatus, Priority)
- [ ] ITaskItemRepository interface

**Application Layer:**
- [ ] DTOs (TaskResponse, CreateTaskRequest, UpdateTaskRequest)
- [ ] CreateTaskCommand, validator, handler
- [ ] GetTaskByIdQuery, handler
- [ ] UpdateTaskCommand, handler
- [ ] DeleteTaskCommand, handler
- [ ] GetTasksByProjectQuery, GetByAssigneeQuery, etc.

**Infrastructure Layer:**
- [ ] TaskItemRepository.cs implementing ITaskItemRepository
- [ ] DependencyInjection.cs

**API Layer:**
- [ ] TasksController.cs
- [ ] Program.cs setup
- [ ] appsettings.json
- [ ] launchSettings.json

**Gateway:**
- [ ] ocelot.json with /api/tasks route

**Testing:**
- [ ] Run `dotnet build` → 0 errors ✓
- [ ] Test create task endpoint
- [ ] Test get task endpoint
- [ ] Test cache working
- [ ] Test Kafka events

---

**Quy trình tương tự dùng cho Project & Notification Services!**
