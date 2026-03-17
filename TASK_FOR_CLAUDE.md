# Implementation Task

Implement Task, Project, Notification services + Gateway config for this .NET 10 microservices project.

## What Already Works
- **Shared libs**: BaseEntity (BsonId), AuditableEntity, IRepository<T>, ICacheService, IKafkaProducer, MongoRepository<T>, RedisCacheService, KafkaProducer, KafkaConsumerBase, ExceptionMiddleware, ServiceCollectionExtensions, Result<T>, PagedResult<T>, ValidationBehavior, LoggingBehavior
- **Auth Service**: COMPLETE — Register, Login, Logout, RefreshToken, GetMe with JWT+BCrypt+MongoDB
- **Enums**: TaskItemStatus(Todo/InProgress/Review/Done), Priority(Low/Medium/High/Urgent), ProjectRole(Member/Admin/Owner), NotificationType(TaskAssigned/TaskCompleted/DueReminder/ProjectUpdate), ReferenceType(Task/Project)

## What Needs Implementation

### 1. Task Service
**Domain** (src/Services/Task/TaskManager.Task.Domain/):
- Entities/TaskItem.cs : AuditableEntity — Title(string), Description(string), ProjectId(string), AssigneeId(string), ReporterId(string), Status(TaskItemStatus), Priority(Priority), DueDate(DateTime?), Tags(List<string>), Comments(List<TaskComment>)
- Entities/TaskComment.cs — Id(string=ObjectId), UserId(string), Content(string), CreatedAt(DateTime)
- Interfaces/ITaskItemRepository.cs : IRepository<TaskItem> + GetByProjectIdAsync, GetByAssigneeIdAsync, GetByStatusAsync

**Application** (src/Services/Task/TaskManager.Task.Application/):
- Commands: CreateTask, UpdateTask, DeleteTask, AddComment, UpdateTaskStatus (each with Command, CommandHandler, Validator)
- Queries: GetTaskById, GetTasksByProject(paged), GetTasksByAssignee, GetMyTasks
- DTOs: TaskResponse, CreateTaskRequest, UpdateTaskRequest

**Infrastructure** (src/Services/Task/TaskManager.Task.Infrastructure/):
- Persistence/TaskItemRepository.cs extending MongoRepository<TaskItem>
- DependencyInjection.cs with AddTaskInfrastructure (MongoDB, Redis, JWT auth, Kafka, repo registration)

**API** (src/Services/Task/TaskManager.Task.API/):
- Controllers/TasksController.cs — full CRUD + status update + add comment. Use MediatR Send.
- Program.cs — follow Auth Service pattern (Serilog, AddApplicationServices, AddTaskInfrastructure, UseSharedMiddleware, auth/authz, health checks)
- appsettings.json — MongoDB(task_db), Redis, Kafka, Jwt settings
- Properties/launchSettings.json — port 5002

### 2. Project Service
**Domain** (src/Services/Project/TaskManager.Project.Domain/):
- Entities/Project.cs : AuditableEntity — Name, Description, OwnerId, Members(List<ProjectMember>), IsArchived(bool)
- Entities/ProjectMember.cs — UserId, Role(ProjectRole), JoinedAt(DateTime)
- Interfaces/IProjectRepository.cs : IRepository<Project> + GetByOwnerIdAsync, GetByMemberIdAsync

**Application** (src/Services/Project/TaskManager.Project.Application/):
- Commands: CreateProject, UpdateProject, DeleteProject, AddMember, RemoveMember, UpdateMemberRole
- Queries: GetProjectById, GetMyProjects(paged), GetProjectMembers
- DTOs: ProjectResponse, CreateProjectRequest, UpdateProjectRequest

**Infrastructure** (src/Services/Project/TaskManager.Project.Infrastructure/):
- Persistence/ProjectRepository.cs
- DependencyInjection.cs with AddProjectInfrastructure

**API** (src/Services/Project/TaskManager.Project.API/):
- Controllers/ProjectsController.cs — CRUD + member management
- Program.cs, appsettings.json(project_db), launchSettings.json(port 5003)

### 3. Notification Service
**Domain** (src/Services/Notification/TaskManager.Notification.Domain/):
- Entities/Notification.cs : AuditableEntity — UserId, Title, Message, Type(NotificationType), ReferenceId, ReferenceType(ReferenceType), IsRead(bool), ReadAt(DateTime?)
- Interfaces/INotificationRepository.cs : IRepository<Notification> + GetByUserIdAsync(paged), GetUnreadCountAsync, MarkAsReadAsync, MarkAllAsReadAsync

**Application** (src/Services/Notification/TaskManager.Notification.Application/):
- Commands: CreateNotification, MarkAsRead, MarkAllAsRead
- Queries: GetNotifications(paged, filterable by isRead), GetUnreadCount
- DTOs: NotificationResponse

**Infrastructure** (src/Services/Notification/TaskManager.Notification.Infrastructure/):
- Persistence/NotificationRepository.cs
- Consumers/NotificationConsumer.cs (extends KafkaConsumerBase, listens to task.assigned, task.completed, project.updated)
- DependencyInjection.cs with AddNotificationInfrastructure (includes Kafka consumer as hosted service)

**API** (src/Services/Notification/TaskManager.Notification.API/):
- Controllers/NotificationsController.cs — GET list, GET unread count, PUT mark read, PUT mark all read
- Program.cs (like Auth + Kafka consumer hosted service), appsettings.json(notification_db), launchSettings.json(port 5004)

### 4. Gateway
- Add src/Gateway/TaskManager.Gateway/ocelot.json routing all 4 services
- Update Program.cs to use Ocelot middleware + JWT auth forwarding
- Port 5000, routes: /api/auth->5001, /api/tasks->5002, /api/projects->5003, /api/notifications->5004

## Rules
1. Follow EXACT same patterns as Auth Service
2. Use MediatR for all commands/queries
3. Use FluentValidation for validators
4. Delete WeatherForecast* boilerplate from all services
5. DO NOT modify Shared or Auth files
6. Inject IKafkaProducer in Task/Project services to publish events
7. Run `dotnet build TaskManager.slnx` and fix ALL errors until 0 errors

When done, run: openclaw system event --text "Done: Implemented Task, Project, Notification services + Gateway. Build succeeded." --mode now
