# Advanced Concepts & Patterns Reference

## 1. CQRS (Command Query Responsibility Segregation)

### Nguyên Tắc

**Tách biệt:**
- **Commands** → Thay đổi state (Create, Update, Delete)
- **Queries** → Lấy dữ liệu (Read-only, không effect side)

### Lợi Ích

```
Without CQRS (Tightly Coupled):
┌─────────────────┐
│  CreateUser     │ → Database update
│  UpdateUser     │ → Kafka publish
│  DeleteUser     │ → Cache invalidate
│  GetUser        │ ← Mix of concerns
└─────────────────┘

With CQRS (Separated):
┌──────────────────────┬──────────────────────┐
│  COMMANDS (Write)    │  QUERIES (Read)      │
├──────────────────────┼──────────────────────┤
│ CreateUserCommand    │ GetUserByIdQuery     │
│ UpdateUserCommand    │ GetAllUsersQuery     │
│ DeleteUserCommand    │ GetUsersByRoleQuery  │
└──────────────────────┴──────────────────────┘
```

### Pattern Flow

```
HTTP Request
    ↓
Controller
    ↓
MediatR.Send(Command/Query)
    ↓
[Validation Behavior]
    • FluentValidation runs
    • If invalid → throw ValidationException
    • ProblemDetails returned to client
    ↓
[Logging Behavior]
    • Log command/query details
    • Log execution time
    ↓
[Handler]
    • Business logic
    • Data access
    • External service calls
    ↓
[Return Result]
    • DTO mapped
    • Cached (if Query)
    • Kafka published (if Command)
```

### Code Example

```csharp
// COMMAND: Write operation
public record CreateTaskCommand(
    string Title,
    string Description) : IRequest<TaskResponse>;

// QUERY: Read operation (no side effects)
public record GetTaskByIdQuery(
    string TaskId) : IRequest<TaskResponse>;

// Both use MediatR's IRequest<TResponse>
// MediatR finds handler that implements IRequestHandler<TCommand, TResponse>
```

---

## 2. Repository Pattern

### Mục Đích

**Abstraction layer** giữa Application & Data Access

```
Application Layer
    ↓
ITaskRepository (Interface)
    ↓
TaskRepository : ITaskRepository (Implementation)
    ↓
MongoDB Driver
    ↓
MongoDB Database
```

### Benefit

| Benefit | Ví Dụ |
|---------|-------|
| **Testability** | Mock repository trong unit tests |
| **Portability** | Swap MongoDB → SQL Server by implementing interface |
| **Consistency** | Tất cả repos implement `IRepository<T>` base interface |
| **DRY** | CRUD logic trong base MongoRepository |

### Implementation Pattern

```csharp
// 1. Base interface (Generic)
public interface IRepository<T> where T : BaseEntity
{
    Task<T?> GetByIdAsync(string id, CancellationToken ct);
    Task<T> AddAsync(T entity, CancellationToken ct);
    Task<T> UpdateAsync(T entity, CancellationToken ct);
    Task<bool> DeleteAsync(string id, CancellationToken ct);
}

// 2. Service-specific interface (extends base)
public interface ITaskRepository : IRepository<TaskItem>
{
    Task<List<TaskItem>> GetByProjectIdAsync(string projectId, CancellationToken ct);
    // Custom queries only
}

// 3. Base implementation (MongoDB generic)
public abstract class MongoRepository<T> : IRepository<T>
{
    protected readonly IMongoCollection<T> _collection;

    public virtual async Task<T?> GetByIdAsync(string id, CancellationToken ct)
    {
        var filter = Builders<T>.Filter.Eq(x => x.Id, id);
        return await _collection.Find(filter).FirstOrDefaultAsync(ct);
    }
    // Other CRUD methods...
}

// 4. Service-specific implementation
public class TaskRepository : MongoRepository<TaskItem>, ITaskRepository
{
    public TaskRepository(IMongoDatabase database)
        : base(database, "tasks")
    {
    }

    // Implement service-specific queries
    public async Task<List<TaskItem>> GetByProjectIdAsync(
        string projectId, 
        CancellationToken ct)
    {
        var filter = Builders<TaskItem>.Filter.Eq(x => x.ProjectId, projectId);
        return await _collection.Find(filter).ToListAsync(ct);
    }
}
```

---

## 3. Dependency Injection (DI)

### Why DI Matters

```csharp
// ❌ BAD: Hard-coded dependency (tight coupling)
public class TaskHandler
{
    public async Task<Task> Handle(CreateTaskCommand request)
    {
        var repo = new MongoRepository();  // ← Hard-coded!
        var kafka = new KafkaProducer();   // ← Hard-coded!
        // ...
    }
}

// ✓ GOOD: Injected dependency (loose coupling)
public class TaskHandler
{
    private readonly ITaskRepository _repo;
    private readonly IKafkaProducer _kafka;

    public TaskHandler(
        ITaskRepository repo,
        IKafkaProducer kafka)  // ← Constructor injection
    {
        _repo = repo;
        _kafka = kafka;
    }
}
```

### DI Container Setup

```csharp
// Program.cs
var builder = WebApplication.CreateBuilder(args);

// Register services
builder.Services.AddScoped<ITaskRepository, TaskRepository>();
builder.Services.AddScoped<IKafkaProducer, KafkaProducer>();

// When TaskHandler is requested, container automatically:
// 1. Creates ITaskRepository instance
// 2. Creates IKafkaProducer instance
// 3. Injects into TaskHandler constructor
```

### Lifetime Scopes

```csharp
// Transient: New instance every time
services.AddTransient<IMyService, MyService>();

// Scoped: One per HTTP request
services.AddScoped<IMyService, MyService>();
// ← Most common for repositories, handlers

// Singleton: One for entire application life
services.AddSingleton<IMyService, MyService>();
// ← Only for thread-safe stateless services
```

---

## 4. Mapping Between Layers

### Entity ≠ DTO

```
⚠️ IMPORTANT: Domain Entity != API Response

┌──────────────────┐
│  Domain Layer    │
│  TaskItem        │
│  - Id            │
│  - Title         │
│  - Title         │ ← May contain sensitive data
│  - ProjectId     │
│  - Secret        │ ← Never expose to API!
│  - EncryptedKey  │
│  - Password      │
└──────────────────┘
        │
        │ Map to
        ▼
┌──────────────────┐
│  Application DTO │
│  TaskResponse    │
│  - Id            │ ← Safe for API
│  - Title         │
│  - ProjectId     │
│  (No secret!)    │
└──────────────────┘
```

### Mapping Implementation

```csharp
// Option 1: Manual mapping (explicit, testable)
private static TaskResponse MapToResponse(TaskItem entity)
{
    return new TaskResponse(
        entity.Id,
        entity.Title,
        entity.Description,
        entity.ProjectId,
        entity.Status.ToString(),
        entity.Priority.ToString());
}

// Option 2: AutoMapper (reduces boilerplate)
var config = new MapperConfiguration(cfg =>
{
    cfg.CreateMap<TaskItem, TaskResponse>();
});
var mapper = config.CreateMapper();
var response = mapper.Map<TaskResponse>(taskItem);
```

---

## 5. Error Handling Strategy

### Structured Exception Handling

```csharp
// 1. Define custom exceptions
public class BusinessException : Exception { }
public class NotFoundException : BusinessException { }
public class UnauthorizedException : BusinessException { }
public class ValidationException : BusinessException { }

// 2. Throw specific exceptions in handlers
public async Task<TaskResponse> Handle(CreateTaskCommand request, ...)
{
    if (!await ProjectExists(request.ProjectId))
        throw new NotFoundException($"Project {request.ProjectId} not found");
}

// 3. Middleware catches & converts to ProblemDetails
app.UseMiddleware<ExceptionMiddleware>();

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
                Title = "Not Found",
                Detail = ex.Message
            });
        }
        catch (ValidationException ex)
        {
            context.Response.StatusCode = StatusCodes.Status400BadRequest;
            await context.Response.WriteAsJsonAsync(new ProblemDetails
            {
                Status = 400,
                Title = "Validation Error",
                Detail = ex.Message
            });
        }
        catch (Exception ex)
        {
            context.Response.StatusCode = StatusCodes.Status500InternalServerError;
            await context.Response.WriteAsJsonAsync(new ProblemDetails
            {
                Status = 500,
                Title = "Internal Server Error"
            });
        }
    }
}
```

### Client receives standard ProblemDetails (RFC 7231)

```json
{
    "status": 404,
    "title": "Not Found",
    "detail": "Task with ID 'abc123' not found"
}
```

---

## 6. Validation Strategy

### FluentValidation Best Practices

```csharp
public class CreateTaskCommandValidator : AbstractValidator<CreateTaskCommand>
{
    public CreateTaskCommandValidator()
    {
        // 1. Not null/empty
        RuleFor(x => x.Title)
            .NotEmpty()
            .WithMessage("Title is required");

        // 2. Length constraints
        RuleFor(x => x.Title)
            .MaximumLength(200)
            .WithMessage("Title must not exceed 200 characters");

        // 3. Pattern matching
        RuleFor(x => x.Email)
            .Matches(@"^[^\s@]+@[^\s@]+\.[^\s@]+$")
            .WithMessage("Invalid email format");

        // 4. Conditional validation
        RuleFor(x => x.DueDate)
            .GreaterThan(DateTime.UtcNow)
            .When(x => x.DueDate.HasValue)
            .WithMessage("DueDate must be in the future");

        // 5. Custom validation logic
        RuleFor(x => x.ProjectId)
            .MustAsync(async (projectId, ct) =>
            {
                return await _projectRepository.ExistsAsync(projectId, ct);
            })
            .WithMessage("Project not found");
    }
}
```

### Validation Pipeline Behavior

```csharp
// Registered in AddApplicationServices
public class ValidationBehavior<TRequest, TResponse> 
    : IPipelineBehavior<TRequest, TResponse>
    where TRequest : IRequest<TResponse>
{
    private readonly IEnumerable<IValidator<TRequest>> _validators;

    public async Task<TResponse> Handle(
        TRequest request,
        RequestHandlerDelegate<TResponse> next,
        CancellationToken cancellationToken)
    {
        // Run all validators
        var failures = _validators
            .SelectMany(v => v.Validate(request).Errors)
            .Where(f => f != null)
            .ToList();

        if (failures.Any())
            throw new ValidationException(failures);

        // Continue to handler
        return await next();
    }
}
```

---

## 7. Async/Await Best Practices

### CancellationToken Pattern

```csharp
// ✓ CORRECT: Async all the way with CancellationToken
public async Task<TaskResponse> Handle(
    GetTaskByIdQuery request,
    CancellationToken cancellationToken)  // ← Always accept this
{
    var task = await _repository.GetByIdAsync(
        request.TaskId, 
        cancellationToken);  // ← Pass through
    return task;
}

// ❌ WRONG: Blocking with .Result (can cause deadlock)
public TaskResponse Handle(GetTaskByIdQuery request)
{
    var task = _repository.GetByIdAsync(request.TaskId).Result; // ← DEADLOCK RISK!
    return task;
}

// ❌ WRONG: Ignoring CancellationToken
public async Task<TaskResponse> Handle(
    GetTaskByIdQuery request,
    CancellationToken cancellationToken)
{
    var task = await _repository.GetByIdAsync(request.TaskId); // ← Missing ct
    return task;
}
```

### Proper Async Exception Handling

```csharp
// ❌ WRONG: Swallowing exceptions
public async Task<TaskResponse> GetTask(string id)
{
    try
    {
        return await _handler.Handle(new GetTaskByIdQuery(id), CancellationToken.None);
    }
    catch { }  // ← Never do this!
}

// ✓ CORRECT: Rethrow or transform exception
public async Task<TaskResponse> GetTask(string id)
{
    try
    {
        return await _handler.Handle(
            new GetTaskByIdQuery(id), 
            CancellationToken.None);
    }
    catch (KeyNotFoundException ex)
    {
        _logger.LogWarning("Task not found: {Id}", id);
        throw new NotFoundException(ex.Message);
    }
}
```

---

## 8. Caching Strategy

### Cache-Aside Pattern (Recommended)

```csharp
public async Task<TaskResponse> GetTask(string id, CancellationToken ct)
{
    var cacheKey = $"task:{id}";

    // 1. Try cache
    var cached = await _cacheService.GetAsync<TaskResponse>(cacheKey, ct);
    if (cached != null)
        return cached;

    // 2. Not in cache → query database
    var task = await _repository.GetByIdAsync(id, ct);
    if (task == null)
        throw new NotFoundException($"Task {id} not found");

    var response = MapToResponse(task);

    // 3. Store in cache (TTL: 1 hour)
    await _cacheService.SetAsync(
        cacheKey,
        response,
        TimeSpan.FromHours(1),
        ct);

    return response;
}
```

### Cache Invalidation on Update

```csharp
public async Task<TaskResponse> UpdateTask(
    UpdateTaskCommand request,
    CancellationToken ct)
{
    // 1. Update database
    var task = await _repository.UpdateAsync(entity, ct);

    // 2. Invalidate related caches
    await _cacheService.RemoveAsync($"task:{request.Id}", ct);
    await _cacheService.RemoveAsync($"project:{task.ProjectId}:tasks", ct);

    return MapToResponse(task);
}
```

---

## 9. Event-Driven Architecture with Kafka

### Producer Side (Task Service)

```csharp
public class CreateTaskCommandHandler : IRequestHandler<CreateTaskCommand, TaskResponse>
{
    private readonly ITaskRepository _repository;
    private readonly IKafkaProducer _kafkaProducer;

    public async Task<TaskResponse> Handle(
        CreateTaskCommand request,
        CancellationToken ct)
    {
        // 1. Create and save task
        var task = new TaskItem { /* ... */ };
        var result = await _repository.AddAsync(task, ct);

        // 2. Publish event for other services
        await _kafkaProducer.ProduceAsync(
            "task.created",  // ← Topic name
            new  // ← Event payload (minimal data)
            {
                id = result.Id,
                title = result.Title,
                assigneeId = result.AssigneeId,
                timestamp = DateTime.UtcNow
            },
            ct);

        return MapToResponse(result);
    }
}
```

### Consumer Side (Notification Service)

```csharp
// Notification Service listens to events
public class NotificationConsumer : KafkaConsumerBase<TaskCreatedEvent>
{
    private readonly INotificationRepository _repository;

    protected override async Task HandleMessage(TaskCreatedEvent message)
    {
        // Create notification for assignee
        var notification = new Notification
        {
            UserId = message.AssigneeId,
            Title = "You were assigned a task",
            Message = $"Task: {message.Title}",
            Type = NotificationType.TaskAssigned,
            ReferenceId = message.Id,
            ReferenceType = ReferenceType.Task
        };

        await _repository.AddAsync(notification);
    }
}

// Register as hosted service in Program.cs
builder.Services.AddHostedService<NotificationConsumer>();
```

### Benefits

```
❌ Direct coupling (BAD):
┌───────────────┐
│ Task Service  │
│ Calls HTTP    │──→ Notification Service
│               │    (If down, Task creation fails)
└───────────────┘

✓ Event-driven (GOOD):
┌───────────────┐
│ Task Service  │──→ Kafka Topic ──→ Notification Service
│ (Async)       │    (Durable)      (Can catch up later)
└───────────────┘
```

---

## 10. Testing Patterns

### Unit Testing with Mocks

```csharp
[TestClass]
public class CreateTaskCommandHandlerTests
{
    private Mock<ITaskRepository> _repositoryMock;
    private Mock<IKafkaProducer> _kafkaMock;
    private CreateTaskCommandHandler _handler;

    [TestInitialize]
    public void Setup()
    {
        _repositoryMock = new Mock<ITaskRepository>();
        _kafkaMock = new Mock<IKafkaProducer>();
        _handler = new CreateTaskCommandHandler(
            _repositoryMock.Object,
            _kafkaMock.Object);
    }

    [TestMethod]
    public async Task Handle_ValidCommand_CreatesTask()
    {
        // Arrange
        var command = new CreateTaskCommand(
            "Test Task",
            "Description",
            "proj123",
            "user123");

        var expectedTask = new TaskItem
        {
            Id = "task123",
            Title = "Test Task"
        };

        _repositoryMock
            .Setup(x => x.AddAsync(It.IsAny<TaskItem>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(expectedTask);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        Assert.AreEqual("task123", result.Id);
        Assert.AreEqual("Test Task", result.Title);

        // Verify Kafka was called
        _kafkaMock.Verify(
            x => x.ProduceAsync(
                "task.created",
                It.IsAny<object>(),
                It.IsAny<CancellationToken>()),
            Times.Once);
    }
}
```

---

## 11. Configuration Management

### Appsettings by Environment

```
appsettings.json                 ← Base (shared)
appsettings.Development.json     ← Local dev (override)
appsettings.Production.json      ← Production (override)
```

**appsettings.json:**
```json
{
  "MongoDB": {
    "ConnectionString": "mongodb://localhost:27017",
    "DatabaseName": "service_db"
  }
}
```

**appsettings.Production.json:**
```json
{
  "MongoDB": {
    "ConnectionString": "mongodb+srv://username:password@cluster.mongodb.net",
    "DatabaseName": "service_db_prod"
  }
}
```

**Access in code:**
```csharp
var connectionString = configuration["MongoDB:ConnectionString"];
var dbName = configuration.GetValue<string>("MongoDB:DatabaseName");
```

---

## 12. Summary: Perfect Microservice Architecture

```
┌────────────────────────────────────────┐
│         API LAYER (Thin Controllers)   │
│  • Extract user/request info           │
│  • Delegate to MediatR                 │
│  • Return DTO                          │
└──────────────┬─────────────────────────┘
               │
               ▼
┌────────────────────────────────────────┐
│      APPLICATION LAYER (Business Logic)│
│  • Commands (Write)                    │
│  • Queries (Read)                      │
│  • Handlers (CQRS)                     │
│  • Validators (FluentValidation)       │
│  • DTOs (API contracts)                │
└──────────────┬─────────────────────────┘
               │
               ▼
┌────────────────────────────────────────┐
│      INFRASTRUCTURE LAYER (Data Access)│
│  • Repository Implementations          │
│  • External Services (Kafka, Cache)    │
│  • DependencyInjection                 │
└──────────────┬─────────────────────────┘
               │
               ▼
┌────────────────────────────────────────┐
│        DOMAIN LAYER (Pure Business)    │
│  • Entities                            │
│  • Enums                               │
│  • Interfaces (no implementation!)     │
└────────────────────────────────────────┘
```

---

**Last Updated:** March 2026
