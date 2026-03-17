# Lộ Trình Học Tập & Tổng Hợp Kiến Thức

## Mục Đích Tài Liệu

Tài liệu này giúp bạn:
1. **Hiểu rõ** cấu trúc toàn project
2. **Xây dựng**các dự án tương tự độc lập
3. **Nắm vững** các pattern và best practices

---

## I. Bước Học Từng Bước

### Tuần 1: Lý Thuyết Cơ Bản

**Mục tiêu:** Hiểu kiến trúc và lý do tại sao chọn các công nghệ này

**Tài liệu:**
- [ ] Đọc: [ARCHITECTURE.md](ARCHITECTURE.md)
  - Sơ đồ hệ thống
  - Tại sao MongoDB chứ không SQLite
  - Microservices breakdown

- [ ] Đọc: [BACKEND-GUIDELINES.md](BACKEND-GUIDELINES.md)
  - Framework targets (.NET 10)
  - Clean Architecture layers
  - Middleware order (rất quan trọng!)

- [ ] Đọc: [HUONG_DAN_CHI_TIET.md](HUONG_DAN_CHI_TIET.md) - Phần 1-4
  - Tổng quan dự án (khái niệm)
  - Architecture tổng thể
  - Các layer chi tiết

**Bài tập:**
- Vẽ lại sơ đồ hệ thống
- Liệt kê 4 layers + trách nhiệm mỗi layer
- Giải thích lợi ích CQRS

---

### Tuần 2: Deep Dive - Domain & Application

**Mục tiêu:** Hiểu cách viết Entity, Command, Query, Validator

**Tài liệu:**
- [ ] Đọc: [HUONG_DAN_CHI_TIET.md](HUONG_DAN_CHI_TIET.md) - Phần 2-3
  - Domain Layer (Entities, Enums)
  - Application Layer (Commands, Handlers, Validators)

- [ ] Đọc: [PATTERNS_ADVANCED.md](PATTERNS_ADVANCED.md) - Section 1-5
  - CQRS pattern chi tiết
  - Repository pattern
  - Dependency Injection

**Bài tập:**
1. **Viết TaskItem Entity:** 
   ```
   - Properties: Title, Description, Status, Priority, Tags, Comments
   - Kế thừa từ AuditableEntity
   - Collections: TaskComment[]
   ```

2. **Viết CreateTaskCommand & Validator:**
   ```
   - Input: Title, Description, ProjectId, AssigneeId
   - Validate: Title not empty (max 200), ProjectId exists
   - Output: TaskResponse
   ```

3. **Viết CreateTaskCommandHandler:**
   ```
   - Save entity to repo
   - Publish Kafka event
   - Return DTO
   ```

---

### Tuần 3: Infrastructure & External Services

**Mục tiêu:** Hiểu MongoDB repositories, Kafka producers, DI setup

**Tài liệu:**
- [ ] Đọc: [HUONG_DAN_CHI_TIET.md](HUONG_DAN_CHI_TIET.md) - Phần 4
  - MongoDB repository implementation
  - DependencyInjection.cs setup

- [ ] Đọc: [PATTERNS_ADVANCED.md](PATTERNS_ADVANCED.md) - Section 7-9
  - Async/Await best practices
  - Caching strategy
  - Event-driven Kafka

**Bài tập:**
1. **Implement TaskItemRepository:**
   ```
   - GetByProjectIdAsync()
   - GetByAssigneeIdAsync()
   - GetByStatusAsync()
   - Sorting, filtering
   ```

2. **Create DependencyInjection.cs:**
   ```
   - Register MongoDB, Redis, Kafka, JWT
   - Register repositories
   - Return IServiceCollection
   ```

3. **Understand Kafka Events:**
   ```
   - Topic: "task.created"
   - Payload: {id, title, assigneeId}
   - Consumer: NotificationService
   ```

---

### Tuần 4: API Layer & Full Integration

**Mục tiêu:** Hiểu Controllers, Program.cs setup, end-to-end flow

**Tài liệu:**
- [ ] Đọc: [HUONG_DAN_CHI_TIET.md](HUONG_DAN_CHI_TIET.md) - Phần 5-7
  - Controller creation
  - Program.cs setup
  - Error handling, testing

- [ ] Đọc: [PATTERNS_ADVANCED.md](PATTERNS_ADVANCED.md) - Section 5-6
  - Error handling strategy
  - Validation strategy
  - Testing patterns

**Bài tập:**
1. **Create TasksController:**
   ```
   - POST /api/v1/tasks (Create)
   - GET /api/v1/tasks/{id} (Get)
   - PUT /api/v1/tasks/{id} (Update)
   - DELETE /api/v1/tasks/{id} (Delete)
   - Inject IMediator
   - Use [Authorize] attributes
   ```

2. **Setup Program.cs:**
   ```
   - Serilog logging
   - MediatR + FluentValidation
   - Infrastructure DI
   - Middleware pipeline (order!)
   - Health checks
   ```

3. **Create appsettings.json:**
   ```
   - MongoDB connection
   - Redis connection
   - JWT settings
   - Kafka bootstrap servers
   ```

---

### Tuần 5: Practicum - Build Task Service from Scratch

**Mục tiêu:** Xây dựng hoàn toàn Task Service như hướng dẫn

**Tài liệu:**
- [ ] Đọc: [HUONG_DAN_THUC_HANH.md](HUONG_DAN_THUC_HANH.md) - Toàn bộ

**Bài tập (Chi tiết từng bước):**

**Phase 1: Create Projects (2 hours)**
```bash
# 1. Create folder structure
mkdir src\Services\Task

# 2. Create 4 projects
cd src\Services\Task
dotnet new classlib -n TaskManager.Task.Domain
dotnet new classlib -n TaskManager.Task.Application
dotnet new classlib -n TaskManager.Task.Infrastructure
dotnet new webapi -n TaskManager.Task.API

# 3. Add project references
# Domain: No references
# Application: references Domain, TaskManager.Shared.Application
# Infrastructure: references Domain, TaskManager.Shared.Infrastructure
# API: references All + TaskManager.Shared.Infrastructure

# 4. Add NuGet packages
cd TaskManager.Task.Application
dotnet add package MediatR
dotnet add package FluentValidation

cd ../TaskManager.Task.Infrastructure
dotnet add package MongoDB.Driver
dotnet add package StackExchange.Redis

# 5. Build
dotnet build
```

**Phase 2: Domain Layer (3 hours)**
```csharp
// Files to create:
- Enums/TaskItemStatus.cs
- Enums/Priority.cs
- Entities/TaskComment.cs
- Entities/TaskItem.cs
- Interfaces/ITaskItemRepository.cs

// Expected: 0 build errors
```

**Phase 3: Application Layer (5 hours)**
```csharp
// Files to create:
- DTOs/TaskResponse.cs
- DTOs/CreateTaskRequest.cs
- DTOs/UpdateTaskRequest.cs
- Commands/CreateTask/CreateTaskCommand.cs
- Commands/CreateTask/CreateTaskCommandValidator.cs
- Commands/CreateTask/CreateTaskCommandHandler.cs
- Queries/GetTaskById/GetTaskByIdQuery.cs
- Queries/GetTaskById/GetTaskByIdQueryHandler.cs
- Commands/UpdateTask/... (similar)
- Commands/DeleteTask/... (similar)
- Queries/GetTasksByProject/... (similar)

// Expected: 0 build errors
```

**Phase 4: Infrastructure Layer (2 hours)**
```csharp
// Files to create:
- Persistence/TaskItemRepository.cs
- DependencyInjection.cs

// Expected: 0 build errors
```

**Phase 5: API Layer (3 hours)**
```csharp
// Files to create:
- Controllers/TasksController.cs
- Program.cs
- appsettings.json
- Properties/launchSettings.json

// Expected: 0 build errors
```

**Phase 6: Testing (2 hours)**
```bash
# 1. Build entire solution
dotnet build TaskManager.slnx
# Expected: No errors

# 2. Run Task Service
cd src/Services/Task/TaskManager.Task.API
dotnet run

# 3. In another terminal, test endpoints
# Register user (Auth Service)
# Create task
# Get task
# Update task
# Delete task
```

---

### Tuần 6-8: Advanced Topics

**Tuần 6: Kafka & Event-Driven**
- [ ] Implement Kafka producer in Task Service
- [ ] Implement Kafka consumer in Notification Service
- [ ] Publish task.created, task.updated, task.deleted events
- [ ] Subscribe and create notifications

**Tuần 7: Caching & Performance**
- [ ] Implement cache-aside pattern in GetTaskByIdQuery
- [ ] Invalidate cache on Update/Delete
- [ ] Benchmark: with cache vs without cache
- [ ] Learn about cache stampede, TTL strategies

**Tuần 8: Project & Notification Services**
- [ ] Apply same pattern to Project Service
- [ ] Apply same pattern to Notification Service
- [ ] Test event flow: Task created → Notification published
- [ ] Update Gateway Ocelot config

---

## II. Cheat Sheet: Tạo Microservice Mới trong 2 Ngày

### Day 1: Structure + Core Logic

**Checklist:**
- [ ] Create 4 projects (Domain, Application, Infrastructure, API)
- [ ] Domain: Entities + Enums + Interfaces
- [ ] Application: Commands/Queries + Handlers + Validators + DTOs
- [ ] Infrastructure: Repository + DependencyInjection
- [ ] API: Controller + Program.cs + appsettings.json
- [ ] Build: `dotnet build` → 0 errors

### Day 2: Testing + Integration

**Checklist:**
- [ ] Run service: `dotnet run`
- [ ] Test endpoints with Postman/curl
- [ ] Integrate with other services
- [ ] Add to Gateway ocelot.json
- [ ] Test end-to-end flow
- [ ] Add logging (Serilog)

---

## III. Project Structure Quick Reference

```
src/Services/[ServiceName]/
├── TaskManager.[ServiceName].Domain/
│   ├── Entities/
│   │   └── *.cs (No dependencies)
│   ├── Enums/
│   │   └── *.cs
│   └── Interfaces/
│       └── I*Repository.cs
│
├── TaskManager.[ServiceName].Application/
│   ├── Commands/
│   │   └── [CommandName]/
│   │       ├── [CommandName].cs
│   │       ├── [CommandName]Validator.cs
│   │       └── [CommandName]Handler.cs
│   ├── Queries/
│   │   └── [QueryName]/
│   │       ├── [QueryName].cs
│   │       └── [QueryName]Handler.cs
│   ├── DTOs/
│   │   └── *Response.cs, *Request.cs
│   └── Interfaces/
│       └── (Service contracts)
│
├── TaskManager.[ServiceName].Infrastructure/
│   ├── Persistence/
│   │   └── *Repository.cs
│   ├── Services/
│   │   └── (External integrations)
│   └── DependencyInjection.cs
│
└── TaskManager.[ServiceName].API/
    ├── Controllers/
    │   └── *Controller.cs
    ├── Program.cs
    ├── appsettings.json
    ├── appsettings.Development.json
    └── Properties/
        └── launchSettings.json
```

---

## IV. Common Mistakes & How to Avoid

| Mistake | Impact | Solution |
|---------|--------|----------|
| **Domain depends on EF/MongoDB** | Not reusable | Keep Domain pure, no framework references |
| **Business logic in Controller** | Hard to test | Move to Handler |
| **Validators in Handler** | Mixed concerns | Use FluentValidation + pipeline behavior |
| **No DTO mapping** | API leaks internal data | Create distinct DTOs |
| **Sync-over-async (.Result)** | Deadlock risk | Always use async/await |
| **No CancellationToken** | Can't cancel requests | Pass ct through all async calls |
| **Hard-coded dependencies** | Not testable | Use constructor injection |
| **No cache invalidation** | Stale data | Invalidate on update/delete |
| **Validate in repository** | Mixing concerns | Validate in handler/validator |
| **No error handling** | Bad UX | Use custom exceptions + middleware |

---

## V. Useful Commands

### Build & Run

```bash
# Build solution
dotnet build TaskManager.slnx

# Run specific service
cd src/Services/Task/TaskManager.Task.API
dotnet run

# Run with specific configuration
dotnet run --configuration Development

# Build release
dotnet build --configuration Release
```

### NuGet Management

```bash
# Add package
dotnet add package MediatR

# List packages
dotnet package search MediatR

# Update package
dotnet package update MediatR
```

### Project Management

```bash
# List projects in solution
dotnet sln list

# Add project to solution
dotnet sln add src/Services/Task/TaskManager.Task.API/TaskManager.Task.API.csproj

# Add project reference (from one project to another)
cd src/Services/Task/TaskManager.Task.Application
dotnet add reference ../TaskManager.Task.Domain/TaskManager.Task.Domain.csproj
```

### Testing

```bash
# Run tests
dotnet test

# Run specific test class
dotnet test --filter "ClassName=CreateTaskCommandHandlerTests"

# Run with coverage
dotnet test /p:CollectCoverage=true
```

---

## VI. Architecture Decision Records (ADRs)

### ADR-1: Why MongoDB over SQL

**Status:** Accepted

**Context:** Need to store flexible task objects with nested comments, tags

**Decision:** Use MongoDB

**Consequences:**
- ✓ Schema flexibility
- ✓ Easier nested documents
- ✗ No ACID transactions (though MongoDB 4.0+ supports some)
- ✗ No SQL for complex joins

---

### ADR-2: Why Microservices

**Status:** Accepted

**Context:** Task Manager has Auth, Task, Project, Notification domains

**Decision:** Split into separate microservices

**Consequences:**
- ✓ Independent deployment
- ✓ Team autonomy
- ✗ Complexity (distributed system)
- ✗ Network latency (Kafka for async communication)

---

### ADR-3: Why CQRS via MediatR

**Status:** Accepted

**Context:** Need consistent command/query handling, validation pipeline

**Decision:** Implement CQRS using MediatR

**Consequences:**
- ✓ Separation of concerns
- ✓ Easy to add behaviors (validation, logging, caching)
- ✗ More boilerplate
- ✗ Learning curve for team

---

## VII. Performance Tips

### 1. Indexing in MongoDB

```csharp
// In repository constructor
var indexOptions = new CreateIndexOptions { Unique = false };
var indexModel = new CreateIndexModel<TaskItem>(
    Builders<TaskItem>.IndexKeys.Ascending(t => t.ProjectId),
    indexOptions);
_collection.Indexes.CreateOne(indexModel);

// Query by ProjectId will be fast
```

### 2. Pagination for Large Lists

```csharp
public async Task<PagedResult<TaskResponse>> GetByProject(
    string projectId,
    int page = 1,
    int pageSize = 10)
{
    var skip = (page - 1) * pageSize;
    
    var items = await _collection
        .Find(f => f.ProjectId == projectId)
        .Skip(skip)
        .Limit(pageSize)
        .ToListAsync();
    
    var total = await _collection.CountDocumentsAsync(f => f.ProjectId == projectId);
    
    return new PagedResult<TaskResponse>(items, total, page, pageSize);
}
```

### 3. Batch Operations

```csharp
// Don't: Multiple updates
foreach (var taskId in taskIds)
{
    await _repository.UpdateAsync(task); // N database calls
}

// Do: Batch update
var updates = taskIds.Select(id => 
    new UpdateOneModel<TaskItem>(
        Builders<TaskItem>.Filter.Eq(t => t.Id, id),
        Builders<TaskItem>.Update.Set(t => t.Status, TaskItemStatus.Done))).ToList();

await _collection.BulkWriteAsync((IEnumerable<WriteModel<TaskItem>>)updates);
```

---

## VIII. Further Learning Resources

### Books

1. **"Clean Architecture" by Robert C. Martin**
   - Foundation for understanding layers
   - ~330 pages, excellent diagrams

2. **"Domain-Driven Design" by Eric Evans**
   - Deep dive into domain modeling
   - ~560 pages, complex but worth it

3. **"Microservices Patterns" by Chris Richardson**
   - How to design microservices correctly
   - Pattern catalog (saga, event sourcing, etc.)

### Online Courses

- **Udemy: "Microservices Architecture"** (~15 hours)
- **LinkedIn Learning: "ASP.NET Core MVC"** (~4 hours)
- **Microsoft Learn: ASP.NET Core** (free, comprehensive)

### Documentation

- [ASP.NET Core Docs](https://docs.microsoft.com/aspnet/core/)
- [MediatR Documentation](https://github.com/jbogard/MediatR)
- [FluentValidation Docs](https://fluentvalidation.net/)
- [MongoDB C# Driver](https://docs.mongodb.com/drivers/csharp/)

---

## IX. Self-Assessment

### After Week 1, you should be able to:
- [ ] Draw and explain the system architecture
- [ ] Explain why MongoDB was chosen over SQL
- [ ] List and describe each layer's responsibility
- [ ] Define CQRS and MediatR

### After Week 2, you should be able to:
- [ ] Write a domain entity with proper inheritance
- [ ] Create a command with validator
- [ ] Implement a handler with business logic
- [ ] Map entities to DTOs manually

### After Week 3, you should be able to:
- [ ] Implement a MongoDB repository
- [ ] Understand dependency injection setup
- [ ] Explain async/await best practices
- [ ] Implement caching strategy

### After Week 4, you should be able to:
- [ ] Build a complete controller
- [ ] Setup Program.cs from scratch
- [ ] Configure appsettings.json
- [ ] Test end-to-end flow

### After Week 5, you should be able to:
- [ ] Create a complete microservice from scratch
- [ ] Integrate with other microservices
- [ ] Publish and consume Kafka events
- [ ] Debug and troubleshoot issues

---

## X. Next Steps After Mastering Task Manager

1. **Build Project & Notification Services**
   - Same pattern as Task Service
   - Estimate: 2 weeks

2. **Build Frontend (React)**
   - Connect to Gateway
   - Consume REST APIs
   - Estimate: 3-4 weeks

3. **Deploy to Production**
   - Docker + Kubernetes
   - CI/CD pipeline
   - Monitoring & logging
   - Estimate: 2 weeks

4. **Advanced Features**
   - SignalR for real-time notifications
   - GraphQL for flexible queries
   - Event sourcing
   - SAGA pattern for distributed transactions

---

**Happy Learning! Remember: Understanding WHY before HOW is key.**

Questions? Review the full documentation in this docs/ folder.

Created: March 2026
