# Hướng Dẫn Chi Tiết: Cấu Trúc Project Task Manager

## Mục Lục
1. [Tổng Quan Dự Án](#tổng-quan-dự-án)
2. [Kiến Trúc Tổng Thể](#kiến-trúc-tổng-thể)
3. [Clean Architecture Layers](#clean-architecture-layers)
4. [Pattern & Technologies](#pattern--technologies)
5. [Hướng Dẫn Xây Dựng Microservice Mới](#hướng-dẫn-xây-dựng-microservice-mới)
6. [Các Khái Niệm Quan Trọng](#các-khái-niệm-quan-trọng)
7. [Tối Ưu & Best Practices](#tối-ưu--best-practices)

---

## 1. Tổng Quan Dự Án

### Mục Đích Chính
**Task Manager** là một ứng dụng **quản lý công việc (task management)** tương tự Todoist/Jira, sử dụng kiến trúc **Microservices** hiện đại.

### Tech Stack
| Thành Phần | Công Nghệ |
|-----------|-----------|
| **Backend** | .NET 10 / ASP.NET Core 10 (C#) |
| **Frontend** | React + TypeScript + Vite |
| **Database** | MongoDB (document-based) |
| **Cache** | Redis |
| **Message Broker** | Apache Kafka |
| **API Gateway** | Ocelot |
| **Design Pattern** | Clean Architecture + CQRS (MediatR) |
| **Authentication** | JWT + Refresh Token + BCrypt |

### Tại Sao Chọn MongoDB?
```
✓ Schema linh hoạt → Dễ mở rộng task có tags, comments, subtasks
✓ Document-based → Tự nhiên lưu nested objects (tasks + comments)
✓ Per-service DB → Mỗi microservice có DB riêng, không share
✓ Horizontal scaling → Phù hợp microservices
✓ Change streams → Tích hợp Kafka dễ dàng
✓ Multi-user concurrency → Built-in handling
```

---

## 2. Kiến Trúc Tổng Thể

### 2.1 Sơ Đồ Hệ Thống

```
┌────────────────────────────────┐
│      FRONTEND (React)           │
│   TypeScript + Vite + Routes   │
└──────────────┬──────────────────┘
               │ HTTPS
               ▼
┌────────────────────────────────┐
│   GATEWAY (Ocelot, Port 5000)  │
│  • Routing: /api/auth -> 5001  │
│  • Routing: /api/tasks -> 5002 │
│  • Routing: /api/projects->5003│
│  • Routing: /api/notif -> 5004 │
└──┬────┬────┬────┬──────────────┘
   │    │    │    │
   ▼    ▼    ▼    ▼
┌─────────────────────────────────────────────────┐
│   MICROSERVICES (Mỗi service, riêng port)       │
├────────┬────────┬──────────┬──────────┐
│ Auth   │ Task   │Notification│Project  │
│ :5001  │ :5002  │ :5003    │ :5004   │
└────┬───┴────┬───┴────┬─────┴────┬────┘
     │        │        │          │
     ▼        ▼        ▼          ▼
┌────────────────────────────────────────┐
│   DATABASES (MongoDB, mỗi service DB)  │
├────────┬────────┬──────────┬─────────┐
│auth_db │task_db │notif_db  │proj_db  │
└────────┴────────┴──────────┴─────────┘

┌────────────────────────────────┐
│  MESSAGE BROKER (Apache Kafka) │
│  • Topic: task.assigned        │
│  • Topic: task.completed       │
│  • Topic: project.updated      │
└────────────────────────────────┘

┌────────────────────────────────┐
│      CACHE (Redis)             │
│  • User sessions               │
│  • Query cache                 │
│  • Rate limiting               │
└────────────────────────────────┘
```

### 2.2 Luồng Request-Response

```
1. User Request
   ↓
2. Gateway (Ocelot) Routes to Service
   ↓
3. Service Receives Request
   ↓
4. Authentication Middleware (JWT validation)
   ↓
5. Authorization Middleware (Check permissions)
   ↓
6. Controller processes request
   ↓
7. MediatR sends Command/Query
   ↓
8. Handler executes business logic
   ↓
9. Service publishes Kafka events (if needed)
   ↓
10. Response returns to Gateway
    ↓
11. Gateway returns to Client
```

---

## 3. Clean Architecture Layers

### 3.1 Cấu Trúc 4 Layers

Mỗi **Microservice** được chia thành 4 project (DLL) riêng biệt:

```
TaskManager.Service/
├── TaskManager.Service.Domain/           ← Layer 1
│   ├── Entities/
│   ├── Enums/
│   ├── ValueObjects/
│   └── Interfaces/
│
├── TaskManager.Service.Application/      ← Layer 2
│   ├── Commands/
│   ├── Queries/
│   ├── DTOs/
│   ├── Interfaces/
│   └── Behaviors/ (Validators, Logging)
│
├── TaskManager.Service.Infrastructure/   ← Layer 3
│   ├── Persistence/ (MongoDB)
│   ├── Services/ (Kafka, JWT)
│   └── DependencyInjection.cs
│
└── TaskManager.Service.API/              ← Layer 4
    ├── Controllers/
    ├── Middleware/
    ├── Program.cs
    └── appsettings.json
```

### 3.2 Layer 1: Domain (Tầng Miền)

**Trách Nhiệm:**
- Định nghĩa **Entities** (các đối tượng cốt lõi)
- Định nghĩa **Enums** (các loại dữ liệu cố định)
- Định nghĩa **Interfaces for repositories** (hợp đồng lưu trữ)

**Đặc Điểm:**
- ✓ **Zero dependencies** → Không phụ thuộc framework, chỉ code thuần C#
- ✓ **Reusable** → Có thể dùng độc lập
- ✓ **Testable** → Dễ viết unit tests

**Ví Dụ: Entity**
```csharp
// Domain Layer - Entities/TaskItem.cs
using TaskManager.Shared.Domain.Entities;

namespace TaskManager.Task.Domain.Entities;

public class TaskItem : AuditableEntity
{
    public string Title { get; set; } = default!;
    public string Description { get; set; } = default!;
    public string ProjectId { get; set; } = default!;
    public string AssigneeId { get; set; } = default!;
    public TaskItemStatus Status { get; set; } = TaskItemStatus.Todo;
    public Priority Priority { get; set; } = Priority.Medium;
    public DateTime? DueDate { get; set; }
    public List<string> Tags { get; set; } = [];
    public List<TaskComment> Comments { get; set; } = [];
}
```

**Ví Dụ: Entity kế thừa**
```csharp
// Domain Layer - dùng base class
public class TaskComment
{
    public string Id { get; set; } = ObjectId.GenerateNewId().ToString();
    public string UserId { get; set; } = default!;
    public string Content { get; set; } = default!;
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
}
```

**Ví Dụ: Enum**
```csharp
// Domain Layer - Enums/TaskItemStatus.cs
namespace TaskManager.Shared.Domain.Enums;

public enum TaskItemStatus
{
    Todo = 0,
    InProgress = 1,
    Review = 2,
    Done = 3
}
```

**Ví Dụ: Repository Interface**
```csharp
// Domain Layer - Interfaces/ITaskItemRepository.cs
using TaskManager.Task.Domain.Entities;
using TaskManager.Shared.Application.Interfaces;

namespace TaskManager.Task.Domain.Interfaces;

public interface ITaskItemRepository : IRepository<TaskItem>
{
    Task<List<TaskItem>> GetByProjectIdAsync(
        string projectId, 
        CancellationToken ct = default);
    
    Task<List<TaskItem>> GetByAssigneeIdAsync(
        string assigneeId, 
        CancellationToken ct = default);
    
    Task<List<TaskItem>> GetByStatusAsync(
        TaskItemStatus status, 
        CancellationToken ct = default);
}
```

### 3.3 Layer 2: Application (Tầng Ứng Dụng)

**Trách Nhiệm:**
- Định nghĩa **Use Cases** (Lệnh/Truy vấn)
- **Validate input** (FluentValidation)
- **Orchestrate** các entity từ Domain
- **DTOs** (Data Transfer Objects) - riêng biệt với Entity

**Pattern: CQRS (Command Query Responsibility Segregation)**
```
Command  → Write data (Create, Update, Delete)
Query    → Read data (Get, Search, Filter)
```

**Ví Dụ: Command & Handler**
```csharp
// Application Layer - Commands/CreateTask/CreateTaskCommand.cs
using MediatR;
using TaskManager.Task.Application.DTOs;

namespace TaskManager.Task.Application.Commands.CreateTask;

// Record = immutable data class (C# 9+)
public record CreateTaskCommand(
    string Title,
    string Description,
    string ProjectId,
    string AssigneeId,
    TaskItemStatus Status = TaskItemStatus.Todo,
    Priority Priority = Priority.Medium,
    DateTime? DueDate = null,
    List<string>? Tags = null
) : IRequest<TaskResponse>;  // ← MediatR: trả về TaskResponse
```

```csharp
// Application Layer - Commands/CreateTask/CreateTaskCommandHandler.cs
using MediatR;
using TaskManager.Task.Domain.Entities;
using TaskManager.Task.Domain.Interfaces;
using TaskManager.Task.Application.DTOs;

namespace TaskManager.Task.Application.Commands.CreateTask;

public class CreateTaskCommandHandler : IRequestHandler<CreateTaskCommand, TaskResponse>
{
    private readonly ITaskItemRepository _repository;
    private readonly IKafkaProducer _kafkaProducer;

    public CreateTaskCommandHandler(
        ITaskItemRepository repository,
        IKafkaProducer kafkaProducer)
    {
        _repository = repository;
        _kafkaProducer = kafkaProducer;
    }

    public async Task<TaskResponse> Handle(
        CreateTaskCommand request,
        CancellationToken cancellationToken)
    {
        // 1. Create entity từ command
        var taskItem = new TaskItem
        {
            Title = request.Title,
            Description = request.Description,
            ProjectId = request.ProjectId,
            AssigneeId = request.AssigneeId,
            Status = request.Status,
            Priority = request.Priority,
            DueDate = request.DueDate,
            Tags = request.Tags ?? []
        };

        // 2. Save to database
        var result = await _repository.AddAsync(taskItem, cancellationToken);

        // 3. Publish event to Kafka (other services listen)
        await _kafkaProducer.ProduceAsync(
            "task.created",
            new { result.Id, result.Title, result.AssigneeId },
            cancellationToken);

        // 4. Return DTO
        return new TaskResponse(
            result.Id,
            result.Title,
            result.Description,
            result.ProjectId,
            result.Status.ToString(),
            result.Priority.ToString(),
            result.DueDate,
            result.Tags);
    }
}
```

**Ví Dụ: FluentValidation Validator**
```csharp
// Application Layer - Commands/CreateTask/CreateTaskCommandValidator.cs
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

**Ví Dụ: Query & Handler**
```csharp
// Application Layer - Queries/GetTaskById/GetTaskByIdQuery.cs
using MediatR;
using TaskManager.Task.Application.DTOs;

namespace TaskManager.Task.Application.Queries.GetTaskById;

public record GetTaskByIdQuery(string TaskId) : IRequest<TaskResponse>;
```

```csharp
// Handler
public class GetTaskByIdQueryHandler : IRequestHandler<GetTaskByIdQuery, TaskResponse>
{
    private readonly ITaskItemRepository _repository;
    private readonly ICacheService _cacheService;

    public async Task<TaskResponse> Handle(
        GetTaskByIdQuery request,
        CancellationToken cancellationToken)
    {
        // 1. Try get from cache
        var cacheKey = $"task:{request.TaskId}";
        var cached = await _cacheService.GetAsync<TaskResponse>(cacheKey, cancellationToken);
        if (cached != null) return cached;

        // 2. Get from database
        var task = await _repository.GetByIdAsync(request.TaskId, cancellationToken);
        if (task == null)
            throw new NotFoundException($"Task {request.TaskId} not found");

        // 3. Map to DTO
        var response = new TaskResponse(...);

        // 4. Cache for 1 hour
        await _cacheService.SetAsync(cacheKey, response, TimeSpan.FromHours(1), cancellationToken);

        return response;
    }
}
```

**Ví Dụ: DTO (Data Transfer Object)**
```csharp
// Application Layer - DTOs/TaskResponse.cs
namespace TaskManager.Task.Application.DTOs;

public record TaskResponse(
    string Id,
    string Title,
    string Description,
    string ProjectId,
    string Status,
    string Priority,
    DateTime? DueDate,
    List<string> Tags);

public record CreateTaskRequest(
    string Title,
    string Description,
    string ProjectId,
    string AssigneeId,
    string Status = "Todo",
    string Priority = "Medium",
    DateTime? DueDate = null,
    List<string>? Tags = null);
```

### 3.4 Layer 3: Infrastructure (Tầng Hạ Tầng)

**Trách Nhiệm:**
- **Implement repositories** với MongoDB
- **External services** (Kafka, JWT, Redis)
- **Dependency Injection** setup
- **Database configuration**

**Ví Dụ: MongoDB Repository Implementation**
```csharp
// Infrastructure Layer - Persistence/TaskItemRepository.cs
using MongoDB.Driver;
using TaskManager.Task.Domain.Entities;
using TaskManager.Task.Domain.Interfaces;
using TaskManager.Shared.Infrastructure.Persistence;

namespace TaskManager.Task.Infrastructure.Persistence;

public class TaskItemRepository : MongoRepository<TaskItem>, ITaskItemRepository
{
    public TaskItemRepository(IMongoDatabase database) 
        : base(database, "tasks")  // ← Collection name
    {
    }

    public async Task<List<TaskItem>> GetByProjectIdAsync(
        string projectId,
        CancellationToken ct = default)
    {
        var filter = Builders<TaskItem>.Filter.Eq(x => x.ProjectId, projectId);
        return await _collection
            .Find(filter)
            .ToListAsync(ct);
    }

    public async Task<List<TaskItem>> GetByAssigneeIdAsync(
        string assigneeId,
        CancellationToken ct = default)
    {
        var filter = Builders<TaskItem>.Filter.Eq(x => x.AssigneeId, assigneeId);
        return await _collection
            .Find(filter)
            .Sort(Builders<TaskItem>.Sort.Descending(x => x.CreatedAt))
            .ToListAsync(ct);
    }

    public async Task<List<TaskItem>> GetByStatusAsync(
        TaskItemStatus status,
        CancellationToken ct = default)
    {
        var filter = Builders<TaskItem>.Filter.Eq(x => x.Status, status);
        return await _collection
            .Find(filter)
            .ToListAsync(ct);
    }
}
```

**Ví Dụ: Dependency Injection Setup**
```csharp
// Infrastructure Layer - DependencyInjection.cs
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

        // Kafka
        services.AddKafkaProducer(configuration);

        // Repositories
        services.AddScoped<ITaskItemRepository, TaskItemRepository>();

        return services;
    }
}
```

### 3.5 Layer 4: API (Tầng Trình Bày)

**Trách Nhiệm:**
- **Controllers** nhận HTTP requests
- **Middleware** (authentication, authorization, exception handling)
- **Program.cs** setup tất cả
- **Configuration files**

**Ví Dụ: Controller**
```csharp
// API Layer - Controllers/TasksController.cs
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using TaskManager.Task.Application.Commands.CreateTask;
using TaskManager.Task.Application.Commands.UpdateTask;
using TaskManager.Task.Application.Commands.DeleteTask;
using TaskManager.Task.Application.Queries.GetTaskById;
using TaskManager.Task.Application.Queries.GetTasksByProject;
using TaskManager.Task.Application.DTOs;

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

    // CREATE
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

    // READ
    [HttpGet("{id}")]
    [ProducesResponseType(typeof(TaskResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetTask(string id)
    {
        var result = await _mediator.Send(new GetTaskByIdQuery(id));
        return Ok(result);
    }

    // READ ALL (with pagination)
    [HttpGet("project/{projectId}")]
    [ProducesResponseType(typeof(PagedResult<TaskResponse>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetTasksByProject(
        string projectId,
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 10)
    {
        var result = await _mediator.Send(
            new GetTasksByProjectQuery(projectId, page, pageSize));
        return Ok(result);
    }

    // UPDATE
    [HttpPut("{id}")]
    [ProducesResponseType(typeof(TaskResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> UpdateTask(
        string id,
        [FromBody] UpdateTaskRequest request)
    {
        var command = new UpdateTaskCommand(
            id,
            request.Title,
            request.Description,
            request.Status,
            request.Priority,
            request.DueDate,
            request.Tags);

        var result = await _mediator.Send(command);
        return Ok(result);
    }

    // DELETE
    [HttpDelete("{id}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> DeleteTask(string id)
    {
        await _mediator.Send(new DeleteTaskCommand(id));
        return NoContent();
    }
}
```

**Ví Dụ: Program.cs**
```csharp
// API Layer - Program.cs
using Serilog;
using TaskManager.Task.Infrastructure;
using TaskManager.Shared.Infrastructure.Extensions;

var builder = WebApplication.CreateBuilder(args);

// ─── Logging ────────────────────────────────────────────
builder.Host.UseSerilog((context, config) =>
{
    config
        .ReadFrom.Configuration(context.Configuration)
        .WriteTo.Console()
        .Enrich.FromLogContext();
});

// ─── Services ───────────────────────────────────────────

// 1. MediatR + FluentValidation
builder.Services.AddApplicationServices(
    typeof(TaskManager.Task.Application.Commands.CreateTask.CreateTaskCommand).Assembly);

// 2. Infrastructure (MongoDB, Redis, Kafka, JWT)
builder.Services.AddTaskInfrastructure(builder.Configuration);

// 3. Controllers + OpenAPI
builder.Services.AddControllers();
builder.Services.AddOpenApi();
builder.Services.AddHealthChecks();

var app = builder.Build();

// ─── Middleware Pipeline (ORDER MATTERS!) ───────────────

// Non-development: Use exception handler + HSTS
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

// Endpoints
app.MapControllers();
app.MapHealthChecks("/health");

app.Run();

public partial class Program { }  // ← For integration tests
```

---

## 4. Pattern & Technologies

### 4.1 MediatR - CQRS Pattern

**MediatR** là một library cho phép:
- Decouple controllers từ handlers
- Reuse logic (cùng một handler có thể dùng từ 2 endpoints)
- Pipeline behaviors (validation, logging)

**Luồng:**
```
Controller
    ↓
MediatR.Send(Command/Query)
    ↓
Validation Behavior (FluentValidation)
    ↓
Logging Behavior
    ↓
Handler Process Logic
    ↓
Return DTO
```

### 4.2 FluentValidation

**Validation** được tách riêng khỏi Handler:

```csharp
// Sai ❌ - Validation trong Handler
public class CreateTaskCommandHandler
{
    public async Task<TaskResponse> Handle(CreateTaskCommand request, ...)
    {
        if (string.IsNullOrEmpty(request.Title))
            throw new ValidationException("Title required");
        // ...
    }
}

// Đúng ✓ - Validation trong Validator
public class CreateTaskCommandValidator : AbstractValidator<CreateTaskCommand>
{
    public CreateTaskCommandValidator()
    {
        RuleFor(x => x.Title).NotEmpty();
    }
}
```

### 4.3 Dependency Injection

**.NET** có built-in DI container. Pattern: Extension methods

```csharp
// In Infrastructure/DependencyInjection.cs
public static IServiceCollection AddTaskInfrastructure(
    this IServiceCollection services,
    IConfiguration configuration)
{
    services.AddScoped<ITaskItemRepository, TaskItemRepository>();
    return services;
}

// In Program.cs
builder.Services.AddTaskInfrastructure(builder.Configuration);
```

### 4.4 MongoDB với Repository Pattern

**Benefit:**
- Abstraction → Dễ swap MongoDB với SQL Server
- Testable → Mock repository trong unit tests
- Consistent interface → Tất cả repositories implement IRepository<T>

**Base MongoRepository** (từ Shared):
```csharp
public abstract class MongoRepository<T> : IRepository<T> where T : BaseEntity
{
    protected readonly IMongoCollection<T> _collection;

    public async Task<T?> GetByIdAsync(string id, CancellationToken ct = default)
    {
        var filter = Builders<T>.Filter.Eq(x => x.Id, id);
        return await _collection.Find(filter).FirstOrDefaultAsync(ct);
    }

    public async Task<T> AddAsync(T entity, CancellationToken ct = default)
    {
        await _collection.InsertOneAsync(entity, null, ct);
        return entity;
    }

    public async Task<T> UpdateAsync(T entity, CancellationToken ct = default)
    {
        var filter = Builders<T>.Filter.Eq(x => x.Id, entity.Id);
        await _collection.ReplaceOneAsync(filter, entity, null, ct);
        return entity;
    }

    public async Task<bool> DeleteAsync(string id, CancellationToken ct = default)
    {
        var filter = Builders<T>.Filter.Eq(x => x.Id, id);
        var result = await _collection.DeleteOneAsync(filter, null, ct);
        return result.DeletedCount > 0;
    }
}
```

### 4.5 JWT Authentication

**Flow:**
```
1. User Register/Login
   ↓
2. Server validates password (BCrypt)
   ↓
3. Server issues JWT token (claims: UserId, Email, etc.)
   ↓
4. Client stores JWT in localStorage
   ↓
5. Client sends JWT in Authorization header: "Bearer <token>"
   ↓
6. Server validates JWT signature
   ↓
7. Request proceeds if valid
```

**JWT Structure:**
```
eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9.{Header}.{Payload}.{Signature}

Header: {"alg":"HS256","typ":"JWT"}
Payload: {"sub":"userId","email":"user@example.com","iat":1644865600}
Signature: HMACSHA256(base64(Header) + "." + base64(Payload), secret)
```

### 4.6 Kafka - Event-Driven Architecture

**Tại Sao Kafka?**
- Decoupling services → Task Service không cần biết Notification Service tồn tại
- Scalability → Có thể thêm consumers mà không ảnh hưởng publishers
- Durability → Messages được lưu, không bị mất nếu service down

**Pattern: Publish Events sau khi create/update**

```csharp
// Task Service: Publish event
await _kafkaProducer.ProduceAsync("task.created", {
    Id = taskId,
    AssigneeId = assigneeId,
    Title = title
});

// Notification Service: Listen & consume
public class NotificationConsumer : KafkaConsumerBase<TaskCreatedEvent>
{
    protected override async Task HandleMessage(TaskCreatedEvent message)
    {
        // Create notification
        var notification = new Notification
        {
            UserId = message.AssigneeId,
            Title = "You were assigned a task",
            Message = $"Task: {message.Title}",
            Type = NotificationType.TaskAssigned,
            ReferenceId = message.Id,
            ReferenceType = ReferenceType.Task
        };
        await _notificationRepository.AddAsync(notification);
    }
}
```

### 4.7 Redis Caching

**Caching Strategy:**

```csharp
// Pattern: Cache-aside
public async Task<TaskResponse> Handle(GetTaskByIdQuery request, ...)
{
    var key = $"task:{request.TaskId}";
    
    // 1. Try get from cache
    var cached = await _cacheService.GetAsync<TaskResponse>(key);
    if (cached != null) return cached;
    
    // 2. Not in cache → get from DB
    var task = await _repository.GetByIdAsync(request.TaskId);
    
    // 3. Put in cache (1 hour TTL)
    await _cacheService.SetAsync(key, task, TimeSpan.FromHours(1));
    
    return task;
}
```

---

## 5. Hướng Dẫn Xây Dựng Microservice Mới

### 5.1 Bước 1: Tạo Solution Structure

```bash
# Tạo 4 projects
dotnet new classlib -n TaskManager.YourService.Domain
dotnet new classlib -n TaskManager.YourService.Application
dotnet new classlib -n TaskManager.YourService.Infrastructure
dotnet new webapi -n TaskManager.YourService.API

# Add references (dependencies)
# API → Infrastructure → Application → Domain
# Infrastructure → Domain
```

### 5.2 Bước 2: Domain Layer

**Tạo entities:**
```csharp
namespace TaskManager.YourService.Domain.Entities;

public class YourEntity : AuditableEntity
{
    public string Name { get; set; } = default!;
    public string Description { get; set; } = default!;
}
```

**Tạo enums (nếu có):**
```csharp
namespace TaskManager.YourService.Domain.Enums;

public enum YourStatus
{
    Active = 0,
    Inactive = 1
}
```

**Tạo repository interface:**
```csharp
namespace TaskManager.YourService.Domain.Interfaces;

public interface IYourEntityRepository : IRepository<YourEntity>
{
    Task<List<YourEntity>> GetByStatusAsync(YourStatus status, CancellationToken ct = default);
}
```

### 5.3 Bước 3: Application Layer

**Tạo command:**
```csharp
// Commands/CreateYourEntity/CreateYourEntityCommand.cs
using MediatR;

namespace TaskManager.YourService.Application.Commands.CreateYourEntity;

public record CreateYourEntityCommand(
    string Name,
    string Description) : IRequest<YourEntityResponse>;
```

**Tạo handler:**
```csharp
// Commands/CreateYourEntity/CreateYourEntityCommandHandler.cs
using MediatR;
using TaskManager.YourService.Domain.Entities;
using TaskManager.YourService.Domain.Interfaces;

namespace TaskManager.YourService.Application.Commands.CreateYourEntity;

public class CreateYourEntityCommandHandler 
    : IRequestHandler<CreateYourEntityCommand, YourEntityResponse>
{
    private readonly IYourEntityRepository _repository;

    public CreateYourEntityCommandHandler(IYourEntityRepository repository)
    {
        _repository = repository;
    }

    public async Task<YourEntityResponse> Handle(
        CreateYourEntityCommand request,
        CancellationToken cancellationToken)
    {
        var entity = new YourEntity
        {
            Name = request.Name,
            Description = request.Description
        };

        var result = await _repository.AddAsync(entity, cancellationToken);

        return new YourEntityResponse(
            result.Id,
            result.Name,
            result.Description);
    }
}
```

**Tạo validator:**
```csharp
// Commands/CreateYourEntity/CreateYourEntityCommandValidator.cs
using FluentValidation;

namespace TaskManager.YourService.Application.Commands.CreateYourEntity;

public class CreateYourEntityCommandValidator 
    : AbstractValidator<CreateYourEntityCommand>
{
    public CreateYourEntityCommandValidator()
    {
        RuleFor(x => x.Name)
            .NotEmpty()
            .MaximumLength(100);

        RuleFor(x => x.Description)
            .MaximumLength(500);
    }
}
```

**Tạo DTO:**
```csharp
// DTOs/YourEntityResponse.cs
namespace TaskManager.YourService.Application.DTOs;

public record YourEntityResponse(
    string Id,
    string Name,
    string Description);
```

### 5.4 Bước 4: Infrastructure Layer

**Implement repository:**
```csharp
// Persistence/YourEntityRepository.cs
using MongoDB.Driver;
using TaskManager.YourService.Domain.Entities;
using TaskManager.YourService.Domain.Interfaces;
using TaskManager.Shared.Infrastructure.Persistence;

namespace TaskManager.YourService.Infrastructure.Persistence;

public class YourEntityRepository : MongoRepository<YourEntity>, IYourEntityRepository
{
    public YourEntityRepository(IMongoDatabase database)
        : base(database, "your_entities")
    {
    }

    public async Task<List<YourEntity>> GetByStatusAsync(
        YourStatus status,
        CancellationToken ct = default)
    {
        var filter = Builders<YourEntity>.Filter.Eq(x => x.Status, status);
        return await _collection.Find(filter).ToListAsync(ct);
    }
}
```

**Tạo DependencyInjection:**
```csharp
// DependencyInjection.cs
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using TaskManager.YourService.Domain.Interfaces;
using TaskManager.YourService.Infrastructure.Persistence;
using TaskManager.Shared.Infrastructure.Extensions;

namespace TaskManager.YourService.Infrastructure;

public static class DependencyInjection
{
    public static IServiceCollection AddYourServiceInfrastructure(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        services.AddMongoDb(configuration);
        services.AddRedisCache(configuration);
        services.AddJwtAuthentication(configuration);

        services.AddScoped<IYourEntityRepository, YourEntityRepository>();

        return services;
    }
}
```

### 5.5 Bước 5: API Layer

**Tạo controller:**
```csharp
// Controllers/YourEntitiesController.cs
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using TaskManager.YourService.Application.Commands.CreateYourEntity;
using TaskManager.YourService.Application.DTOs;

namespace TaskManager.YourService.API.Controllers;

[ApiController]
[Route("api/v1/[controller]")]
[Authorize]
public class YourEntitiesController : ControllerBase
{
    private readonly IMediator _mediator;

    public YourEntitiesController(IMediator mediator)
    {
        _mediator = mediator;
    }

    [HttpPost]
    public async Task<IActionResult> Create([FromBody] CreateYourEntityCommand command)
    {
        var result = await _mediator.Send(command);
        return CreatedAtAction(nameof(Get), new { id = result.Id }, result);
    }

    [HttpGet("{id}")]
    public async Task<IActionResult> Get(string id)
    {
        // TODO: Implement GetByIdQuery
        return Ok();
    }
}
```

**Tạo Program.cs:**
```csharp
// Program.cs
using Serilog;
using TaskManager.YourService.Infrastructure;
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
    typeof(TaskManager.YourService.Application.Commands.CreateYourEntity.CreateYourEntityCommand).Assembly);

builder.Services.AddYourServiceInfrastructure(builder.Configuration);

var app = builder.Build();

if (!app.Environment.IsDevelopment())
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
```

**Tạo appsettings.json:**
```json
{
  "Serilog": {
    "MinimumLevel": "Information"
  },
  "MongoDB": {
    "ConnectionString": "mongodb://localhost:27017",
    "DatabaseName": "yourservice_db"
  },
  "Redis": {
    "ConnectionString": "localhost:6379"
  },
  "Jwt": {
    "Secret": "your-super-secret-key-min-32-characters-long!!!",
    "Issuer": "task-manager",
    "Audience": "task-manager-api",
    "ExpirationMinutes": 60
  },
  "Kafka": {
    "BootstrapServers": "localhost:9092"
  }
}
```

**Tạo launchSettings.json:**
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
      "applicationUrl": "https://localhost:5005;http://localhost:5006"
    }
  }
}
```

---

## 6. Các Khái Niệm Quan Trọng

### 6.1 BaseEntity vs AuditableEntity

```csharp
// BaseEntity - Căn bản, tất cả entities cần có
public abstract class BaseEntity
{
    public string Id { get; set; } = ObjectId.GenerateNewId().ToString();
}

// AuditableEntity - Extends BaseEntity, thêm audit fields
public abstract class AuditableEntity : BaseEntity
{
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
    // Có thể thêm: CreatedBy, UpdatedBy
}
```

### 6.2 IRepository<T> - Generic Repository Interface

```csharp
public interface IRepository<T> where T : BaseEntity
{
    Task<T?> GetByIdAsync(string id, CancellationToken ct = default);
    Task<T> AddAsync(T entity, CancellationToken ct = default);
    Task<T> UpdateAsync(T entity, CancellationToken ct = default);
    Task<bool> DeleteAsync(string id, CancellationToken ct = default);
    Task<List<T>> GetAllAsync(CancellationToken ct = default);
}
```

**Benefit:** Không cần viết CRUD cơ bản lặp lại.

### 6.3 Pagination

**Ở Application Layer:**
```csharp
public record GetTasksByProjectQuery(
    string ProjectId,
    int Page = 1,
    int PageSize = 10) : IRequest<PagedResult<TaskResponse>>;
```

**Ở Handler:**
```csharp
public async Task<PagedResult<TaskResponse>> Handle(...)
{
    var filter = Builders<TaskItem>.Filter.Eq(x => x.ProjectId, request.ProjectId);
    
    var total = await _collection.CountDocumentsAsync(filter);
    var items = await _collection
        .Find(filter)
        .Skip((request.Page - 1) * request.PageSize)
        .Limit(request.PageSize)
        .ToListAsync();

    return new PagedResult<TaskResponse>(
        items.Select(x => new TaskResponse(...)).ToList(),
        total,
        request.Page,
        request.PageSize);
}
```

**PagedResult DTO:**
```csharp
public record PagedResult<T>(
    List<T> Items,
    long Total,
    int Page,
    int PageSize)
{
    public int TotalPages => (int)Math.Ceiling((double)Total / PageSize);
    public bool HasNextPage => Page < TotalPages;
    public bool HasPreviousPage => Page > 1;
}
```

### 6.4 Exception Handling

**Custom Exceptions:**
```csharp
public class NotFoundException : Exception
{
    public NotFoundException(string message) : base(message) { }
}

public class UnauthorizedException : Exception
{
    public UnauthorizedException(string message) : base(message) { }
}
```

**Middleware catches & returns ProblemDetails:**
```csharp
public class ExceptionMiddleware
{
    public async Task InvokeAsync(HttpContext context)
    {
        try
        {
            await _next(context);
        }
        catch (NotFoundException ex)
        {
            context.Response.StatusCode = StatusCodes.Status404NotFound;
            await context.Response.WriteAsJsonAsync(new ProblemDetails 
            { 
                Status = 404, 
                Detail = ex.Message 
            });
        }
        catch (Exception ex)
        {
            context.Response.StatusCode = StatusCodes.Status500InternalServerError;
            await context.Response.WriteAsJsonAsync(new ProblemDetails 
            { 
                Status = 500, 
                Detail = "Internal server error" 
            });
        }
    }
}
```

### 6.5 Result<T> Pattern

**Thay vì throw exception, return Result:**

```csharp
public record Result<T>
{
    public T? Data { get; init; }
    public bool IsSuccess { get; init; }
    public string? Error { get; init; }

    public static Result<T> Success(T data) => new() { Data = data, IsSuccess = true };
    public static Result<T> Failure(string error) => new() { Error = error, IsSuccess = false };
}

// Usage in handler
var task = await _repository.GetByIdAsync(id);
return task == null 
    ? Result<TaskResponse>.Failure("Task not found")
    : Result<TaskResponse>.Success(new TaskResponse(...));
```

---

## 7. Tối Ưu & Best Practices

### 7.1 Validation Best Practices

```csharp
// ❌ Sai: Validation trong handler
public async Task<TaskResponse> Handle(CreateTaskCommand request, ...)
{
    if (request.Title == null) throw new Exception("Title required");
}

// ✓ Đúng: Validation trong validator
public class CreateTaskCommandValidator : AbstractValidator<CreateTaskCommand>
{
    public CreateTaskCommandValidator()
    {
        RuleFor(x => x.Title).NotEmpty().WithMessage("Title is required");
    }
}
```

### 7.2 Async/Await Best Practices

```csharp
// ❌ Sai: sync-over-async
public TaskResponse Handle(CreateTaskCommand request) => 
    _repository.AddAsync(entity).Result;  // ← Deadlock risk!

// ✓ Đúng: async all the way
public async Task<TaskResponse> Handle(
    CreateTaskCommand request,
    CancellationToken cancellationToken) =>
    await _repository.AddAsync(entity, cancellationToken);
```

### 7.3 Dependency Injection Best Practices

```csharp
// ❌ Sai: Service Locator
public class TaskHandler
{
    public async Task Handle(...)
    {
        var repo = container.Resolve<IRepository>();  // ← Hidden dependency
    }
}

// ✓ Đúng: Constructor Injection
public class TaskHandler
{
    private readonly ITaskRepository _repository;

    public TaskHandler(ITaskRepository repository)  // ← Explicit dependency
    {
        _repository = repository;
    }
}
```

### 7.4 Caching Best Practices

```csharp
// ❌ Sai: Cache tất cả
public async Task<TaskResponse> GetTask(string id)
{
    return await _cacheService.GetAsync($"task:{id}");  // ← Infinite cache
}

// ✓ Đúng: Cache với TTL, invalidate on update
public async Task<TaskResponse> GetTask(string id)
{
    var key = $"task:{id}";
    var cached = await _cacheService.GetAsync<TaskResponse>(key);
    if (cached != null) return cached;

    var task = await _repository.GetByIdAsync(id);
    await _cacheService.SetAsync(key, task, TimeSpan.FromHours(1));
    return task;
}

// Invalidate on update
[HttpPut("{id}")]
public async Task<IActionResult> UpdateTask(string id, ...)
{
    var result = await _mediator.Send(new UpdateTaskCommand(...));
    
    // Invalidate cache
    await _cacheService.RemoveAsync($"task:{id}");
    
    return Ok(result);
}
```

### 7.5 Naming Conventions

| Thành Phần | Convention | Ví Dụ |
|-----------|-----------|-------|
| **Class** | PascalCase | `TaskItem`, `UserService` |
| **Interface** | I + PascalCase | `ITaskRepository`, `IJwtService` |
| **Method** | PascalCase | `GetTaskById()`, `CreateTask()` |
| **Variable** | camelCase | `taskId`, `userName` |
| **Constant** | UPPER_SNAKE_CASE | `MAX_TITLE_LENGTH`, `DEFAULT_PAGE_SIZE` |
| **Table/Collection** | snake_case | `tasks`, `refresh_tokens` |
| **DTO** | Entity + `Request/Response` | `CreateTaskRequest`, `TaskResponse` |
| **Repository Method** | `GetBy[Field]Async` | `GetByProjectIdAsync()`, `GetByAssigneeIdAsync()` |

### 7.6 Security Best Practices

```csharp
// ❌ Sai: Plain text password
public async Task<User> Register(string email, string password)
{
    var user = new User { Email = email, PasswordHash = password };  // ← Dangerous
    await _repository.AddAsync(user);
}

// ✓ Đúng: Hash password
public async Task<User> Register(string email, string password)
{
    var hashedPassword = BCrypt.Net.BCrypt.HashPassword(password);
    var user = new User { Email = email, PasswordHash = hashedPassword };
    await _repository.AddAsync(user);
}

// ❌ Sai: JWT secret hard-coded
var secret = "my-secret-key";

// ✓ Đúng: JWT secret from config
var secret = configuration["Jwt:Secret"];

// ❌ Sai: No authorization check
[HttpDelete("{id}")]
public async Task<IActionResult> DeleteTask(string id)
{
    await _mediator.Send(new DeleteTaskCommand(id));
    return Ok();
}

// ✓ Đúng: Check user ownership
[HttpDelete("{id}")]
[Authorize]
public async Task<IActionResult> DeleteTask(string id)
{
    var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
    var task = await _taskRepository.GetByIdAsync(id);
    
    if (task.ReporterId != userId)
        return Forbid();
    
    await _mediator.Send(new DeleteTaskCommand(id));
    return Ok();
}
```

### 7.7 Error Handling Best Practices

```csharp
// ❌ Sai: Generic exception
throw new Exception("Something went wrong");

// ✓ Đúng: Specific exception messages
if (task == null)
    throw new NotFoundException($"Task with ID '{request.Id}' not found");

// Log errors
_logger.LogError("Failed to create task for user {UserId}: {Error}", userId, ex.Message);
```

---

## 8. Checklist: Tạo Microservice Mới

- [ ] Tạo 4 projects: Domain, Application, Infrastructure, API
- [ ] Domain: Tạo Entities, Enums, Interfaces (Repository)
- [ ] Application: Tạo Commands/Queries, Handlers, Validators, DTOs
- [ ] Infrastructure: Implement Repository, DependencyInjection.cs
- [ ] API: Tạo Controller, Program.cs, appsettings.json, launchSettings.json
- [ ] Thêm project references (correct layer dependencies)
- [ ] Thêm NuGet packages: MediatR, FluentValidation, MongoDB.Driver, StackExchange.Redis
- [ ] Implement CRUD operations (Create, Read, Update, Delete)
- [ ] Add caching for frequently accessed data (GetById queries)
- [ ] Add Kafka producer if service emits events
- [ ] Add Kafka consumer if service subscribes to events
- [ ] Add JWT authentication/authorization
- [ ] Add logging (Serilog)
- [ ] Add health check endpoint
- [ ] Update Gateway ocelot.json với service route
- [ ] Test bằng integrate tests
- [ ] Run `dotnet build` - ensure 0 errors

---

## 9. Tài Liệu Tham Khảo

| Topic | Tài Liệu |
|-------|----------|
| **Clean Architecture** | Robert C. Martin's "Clean Architecture: A Craftsman's Guide to Software Structure and Design" |
| **CQRS Pattern** | Martin Fowler: https://martinfowler.com/bliki/CQRS.html |
| **MediatR** | Jimmy Bogard: https://github.com/jbogard/MediatR |
| **FluentValidation** | https://fluentvalidation.net/ |
| **MongoDB C# Driver** | https://www.mongodb.com/docs/drivers/csharp/ |
| **ASP.NET Core** | https://learn.microsoft.com/en-us/aspnet/core/ |
| **JWT** | https://jwt.io/ |
| **Kafka** | https://kafka.apache.org/documentation/ |

---

**Created:** March 2026
**Version:** 1.0
