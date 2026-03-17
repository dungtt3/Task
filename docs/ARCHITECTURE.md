# Task Manager — Architecture Document

## 1. Overview

**Goal:** Full-featured task management app — Todoist simplicity meets Jira-lite power.

**Stack:**
- **Frontend:** React + TypeScript + Vite
- **Backend:** .NET 8 (C#), Clean Architecture, Microservices
- **Message Broker:** Apache Kafka
- **API Gateway:** Ocelot (or YARP)
- **Database:** MongoDB (primary) + Redis (cache)
- **Auth:** JWT + Refresh Token

---

## 2. Why MongoDB over SQLite

| Criteria | SQLite | MongoDB | Winner |
|----------|--------|---------|--------|
| Schema flexibility (tasks, subtasks, tags) | Rigid | Flexible documents | MongoDB |
| Microservice per-service DB | Shared file lock issues | Native per-service | MongoDB |
| Horizontal scaling | ❌ | ✅ | MongoDB |
| Query flexibility (nested subtasks) | Complex JOINs | Natural nesting | MongoDB |
| Event sourcing patterns | Manual | Change streams → Kafka | MongoDB |
| Multi-user concurrent access | Write locks | Built-in | MongoDB |

**Decision:** MongoDB — fits microservice + document-heavy task model perfectly.
Redis handles caching, rate limiting, and session management.

---

## 3. System Architecture

```
┌─────────────────────────────────────────────────────────┐
│                     FRONTEND (React)                     │
│              Vite + TypeScript + TailwindCSS              │
│         React Query · React DnD · React Router           │
└──────────────────────┬──────────────────────────────────┘
                       │ HTTPS
                       ▼
┌─────────────────────────────────────────────────────────┐
│                   API GATEWAY (Ocelot)                   │
│            Routing · Load Balancing · CORS                │
│                   Port: 5000                              │
└──────┬────────┬────────┬────────┬───────────────────────┘
       │        │        │        │
       ▼        ▼        ▼        ▼
┌──────────┐ ┌──────────┐ ┌──────────┐ ┌──────────────────┐
│  Auth    │ │  Task    │ │  Notif   │ │  Project         │
│  Service │ │  Service │ │  Service │ │  Service         │
│  :5001   │ │  :5002   │ │  :5003   │ │  :5004           │
└────┬─────┘ └────┬─────┘ └────┬─────┘ └────┬─────────────┘
     │            │            │              │
     ▼            ▼            ▼              ▼
┌──────────┐ ┌──────────┐ ┌──────────┐ ┌──────────────────┐
│ MongoDB  │ │ MongoDB  │ │ MongoDB  │ │ MongoDB          │
│ auth_db  │ │ task_db  │ │ notif_db │ │ project_db       │
└──────────┘ └──────────┘ └──────────┘ └──────────────────┘
                       │
                       ▼
              ┌────────────────┐
              │  Apache Kafka  │
              │  Event Bus     │
              └────────────────┘
                       │
                       ▼
              ┌────────────────┐
              │     Redis      │
              │ Cache + Rate   │
              │ Limit + Session│
              └────────────────┘
```

---

## 4. Microservices Breakdown

### 4.1 Auth Service (Port 5001)
- **Responsibility:** User registration, login, JWT issuance, refresh tokens
- **DB:** MongoDB `auth_db` → Users collection
- **Middleware stack:**
  ```
  Request → Rate Limit → Validation → Controller → Response
  ```
- **Endpoints:**
  - `POST /api/auth/register`
  - `POST /api/auth/login`
  - `POST /api/auth/refresh`
  - `POST /api/auth/logout`
  - `GET /api/auth/me`

### 4.2 Task Service (Port 5002)
- **Responsibility:** CRUD tasks, subtasks, kanban status, priorities, tags, due dates
- **DB:** MongoDB `task_db` → Tasks, Subtasks collections
- **Middleware stack:**
  ```
  Request → Authentication → Rate Limit → Cache → Validation → Controller → Response
  ```
- **Endpoints:**
  - `GET /api/tasks` (filter, sort, paginate)
  - `GET /api/tasks/{id}`
  - `POST /api/tasks`
  - `PUT /api/tasks/{id}`
  - `PATCH /api/tasks/{id}/status` (Kanban move)
  - `DELETE /api/tasks/{id}`
  - `POST /api/tasks/{id}/subtasks`
  - `PUT /api/tasks/{id}/subtasks/{subId}`
  - `DELETE /api/tasks/{id}/subtasks/{subId}`
- **Kafka Events Published:**
  - `task.created`, `task.updated`, `task.completed`, `task.deleted`
  - `task.due-approaching` (scheduled check)

### 4.3 Project Service (Port 5004)
- **Responsibility:** Project/category CRUD, member management
- **DB:** MongoDB `project_db` → Projects collection
- **Middleware stack:**
  ```
  Request → Authentication → Rate Limit → Cache → Validation → Controller → Response
  ```
- **Endpoints:**
  - `GET /api/projects`
  - `GET /api/projects/{id}`
  - `POST /api/projects`
  - `PUT /api/projects/{id}`
  - `DELETE /api/projects/{id}`
  - `POST /api/projects/{id}/members`
- **Kafka Events Published:**
  - `project.created`, `project.updated`, `project.deleted`

### 4.4 Notification Service (Port 5003)
- **Responsibility:** Consume Kafka events, push notifications, email, in-app
- **DB:** MongoDB `notif_db` → Notifications collection
- **Middleware stack:**
  ```
  Request → Authentication → Rate Limit → Controller → Response
  ```
- **Kafka Events Consumed:**
  - `task.created`, `task.completed`, `task.due-approaching`
  - `project.updated`
- **Endpoints:**
  - `GET /api/notifications` (user's notifications)
  - `PATCH /api/notifications/{id}/read`
  - `DELETE /api/notifications/{id}`
- **Push:** SignalR WebSocket for real-time

---

## 5. Clean Architecture (Per Service)

```
src/Services/TaskService/
├── TaskService.Domain/           # Entities, Value Objects, Interfaces
│   ├── Entities/
│   │   ├── TaskItem.cs
│   │   └── SubTask.cs
│   ├── Enums/
│   │   ├── TaskStatus.cs        # Todo, InProgress, Review, Done
│   │   ├── Priority.cs          # Low, Medium, High, Urgent
│   │   └── ...
│   ├── Interfaces/
│   │   ├── ITaskRepository.cs
│   │   └── IUnitOfWork.cs
│   └── Events/
│       └── TaskCreatedEvent.cs
│
├── TaskService.Application/      # Use Cases, DTOs, Validators
│   ├── Commands/
│   │   ├── CreateTask/
│   │   │   ├── CreateTaskCommand.cs
│   │   │   ├── CreateTaskHandler.cs
│   │   │   └── CreateTaskValidator.cs
│   │   ├── UpdateTask/
│   │   └── DeleteTask/
│   ├── Queries/
│   │   ├── GetTasks/
│   │   └── GetTaskById/
│   ├── DTOs/
│   │   ├── TaskDto.cs
│   │   └── CreateTaskRequest.cs
│   ├── Interfaces/
│   │   ├── IKafkaProducer.cs
│   │   └── ICacheService.cs
│   └── Behaviors/               # MediatR pipeline behaviors
│       ├── ValidationBehavior.cs
│       └── LoggingBehavior.cs
│
├── TaskService.Infrastructure/   # MongoDB, Kafka, Redis implementations
│   ├── Persistence/
│   │   ├── MongoDbContext.cs
│   │   └── TaskRepository.cs
│   ├── Messaging/
│   │   ├── KafkaProducer.cs
│   │   └── KafkaConsumer.cs
│   ├── Caching/
│   │   └── RedisCacheService.cs
│   └── DependencyInjection.cs
│
└── TaskService.API/              # Controllers, Middleware, Startup
    ├── Controllers/
    │   └── TaskController.cs
    ├── Middleware/
    │   ├── AuthenticationMiddleware.cs
    │   ├── RateLimitMiddleware.cs
    │   ├── CacheMiddleware.cs
    │   └── ExceptionMiddleware.cs
    ├── Filters/
    │   └── ValidationFilter.cs
    └── Program.cs
```

---

## 6. Middleware Pipeline (Per Service)

```csharp
// Program.cs — middleware order matters!
app.UseMiddleware<ExceptionMiddleware>();      // Global error handling
app.UseMiddleware<AuthenticationMiddleware>(); // JWT validation
app.UseMiddleware<RateLimitMiddleware>();      // Redis-based rate limiting  
app.UseMiddleware<CacheMiddleware>();          // Redis response caching
// Then MVC pipeline with validation filters
```

**Rate Limiting Strategy:**
- Per-user: 100 req/min (authenticated)
- Per-IP: 30 req/min (unauthenticated)
- Stored in Redis with sliding window

**Cache Strategy:**
- GET endpoints: 30s cache (Redis)
- Cache invalidation on write operations via Kafka events
- Cache key: `{service}:{userId}:{endpoint}:{queryHash}`

---

## 7. Data Models (MongoDB Documents)

### User (auth_db)
```json
{
  "_id": "ObjectId",
  "email": "string",
  "passwordHash": "string",
  "displayName": "string",
  "avatar": "string?",
  "refreshTokens": [{ "token": "string", "expires": "DateTime", "created": "DateTime" }],
  "createdAt": "DateTime",
  "updatedAt": "DateTime"
}
```

### Task (task_db)
```json
{
  "_id": "ObjectId",
  "title": "string",
  "description": "string?",
  "status": "Todo | InProgress | Review | Done",
  "priority": "Low | Medium | High | Urgent",
  "tags": ["string"],
  "projectId": "ObjectId?",
  "assigneeId": "ObjectId",
  "creatorId": "ObjectId",
  "dueDate": "DateTime?",
  "completedAt": "DateTime?",
  "subtasks": [
    {
      "_id": "ObjectId",
      "title": "string",
      "isCompleted": false,
      "order": 0
    }
  ],
  "order": 0,
  "createdAt": "DateTime",
  "updatedAt": "DateTime"
}
```

### Project (project_db)
```json
{
  "_id": "ObjectId",
  "name": "string",
  "description": "string?",
  "color": "string",
  "icon": "string?",
  "ownerId": "ObjectId",
  "members": [{ "userId": "ObjectId", "role": "Owner | Admin | Member" }],
  "createdAt": "DateTime",
  "updatedAt": "DateTime"
}
```

### Notification (notif_db)
```json
{
  "_id": "ObjectId",
  "userId": "ObjectId",
  "type": "TaskAssigned | TaskCompleted | DueReminder | ProjectUpdate",
  "title": "string",
  "message": "string",
  "referenceId": "ObjectId?",
  "referenceType": "Task | Project",
  "isRead": false,
  "createdAt": "DateTime"
}
```

---

## 8. Frontend Architecture

```
src/
├── app/
│   ├── store.ts                  # Redux Toolkit store
│   └── api/                      # RTK Query API slices
│       ├── authApi.ts
│       ├── taskApi.ts
│       ├── projectApi.ts
│       └── notificationApi.ts
├── features/
│   ├── auth/
│   │   ├── LoginPage.tsx
│   │   ├── RegisterPage.tsx
│   │   └── authSlice.ts
│   ├── tasks/
│   │   ├── TaskList.tsx
│   │   ├── TaskCard.tsx
│   │   ├── TaskDetail.tsx
│   │   ├── KanbanBoard.tsx
│   │   ├── KanbanColumn.tsx
│   │   └── TaskForm.tsx
│   ├── projects/
│   │   ├── ProjectList.tsx
│   │   ├── ProjectDetail.tsx
│   │   └── ProjectForm.tsx
│   └── notifications/
│       ├── NotificationBell.tsx
│       └── NotificationList.tsx
├── components/
│   ├── Layout/
│   │   ├── Sidebar.tsx
│   │   ├── Header.tsx
│   │   └── MainLayout.tsx
│   ├── ui/                       # Reusable UI components
│   │   ├── Button.tsx
│   │   ├── Modal.tsx
│   │   ├── Badge.tsx
│   │   └── ...
│   └── DragDrop/
│       └── SortableItem.tsx
├── hooks/
│   ├── useAuth.ts
│   └── useSignalR.ts
├── lib/
│   ├── axios.ts
│   └── signalr.ts
└── styles/
    └── globals.css               # Tailwind base
```

**Key Libraries:**
- `@dnd-kit/core` — Drag & drop (Kanban)
- `@reduxjs/toolkit` + RTK Query — State + API caching
- `react-router-dom` v6 — Routing
- `tailwindcss` — Styling
- `lucide-react` — Icons
- `date-fns` — Date handling
- `@microsoft/signalr` — Real-time notifications

---

## 9. Development Phases

### Phase 1: Foundation (Week 1-2)
- [ ] Solution structure + Clean Architecture scaffolding
- [ ] Docker Compose (MongoDB, Redis, Kafka, Zookeeper)
- [ ] API Gateway setup (Ocelot)
- [ ] Auth Service (register, login, JWT, refresh)
- [ ] Shared libraries (middleware, common models)
- [ ] React project setup (Vite + TailwindCSS + routing)
- [ ] Login/Register pages

### Phase 2: Core Features (Week 3-4)
- [ ] Task Service (full CRUD + subtasks)
- [ ] Project Service (CRUD + members)
- [ ] Frontend: Task list view + Task form
- [ ] Frontend: Project management
- [ ] Redis caching integration
- [ ] Rate limiting middleware

### Phase 3: Kanban & UX (Week 5)
- [ ] Kanban board with drag & drop
- [ ] Task filtering, sorting, search
- [ ] Tags management
- [ ] Priority & due date UI
- [ ] Responsive design

### Phase 4: Real-time & Notifications (Week 6)
- [ ] Kafka event pipeline
- [ ] Notification Service
- [ ] SignalR real-time push
- [ ] In-app notification center
- [ ] Due date reminders (background job)

### Phase 5: Polish (Week 7)
- [ ] Dark mode
- [ ] Performance optimization
- [ ] Error handling & loading states
- [ ] Testing (unit + integration)
- [ ] Documentation

---

## 10. Infrastructure (Docker Compose)

```yaml
services:
  mongodb:
    image: mongo:7
    ports: ["27017:27017"]
    volumes: [mongo-data:/data/db]

  redis:
    image: redis:7-alpine
    ports: ["6379:6379"]

  zookeeper:
    image: confluentinc/cp-zookeeper:7.5.0
    ports: ["2181:2181"]

  kafka:
    image: confluentinc/cp-kafka:7.5.0
    ports: ["9092:9092"]
    depends_on: [zookeeper]

  api-gateway:
    build: ./src/Gateway
    ports: ["5000:5000"]
    depends_on: [auth-service, task-service, project-service, notification-service]

  auth-service:
    build: ./src/Services/AuthService
    ports: ["5001:5001"]
    depends_on: [mongodb, redis]

  task-service:
    build: ./src/Services/TaskService
    ports: ["5002:5002"]
    depends_on: [mongodb, redis, kafka]

  notification-service:
    build: ./src/Services/NotificationService
    ports: ["5003:5003"]
    depends_on: [mongodb, kafka]

  project-service:
    build: ./src/Services/ProjectService
    ports: ["5004:5004"]
    depends_on: [mongodb, redis, kafka]

  frontend:
    build: ./src/Frontend
    ports: ["3000:3000"]
    depends_on: [api-gateway]
```

---

## 11. Key Patterns & Principles

- **CQRS** via MediatR (Commands/Queries separated)
- **Repository Pattern** with MongoDB driver
- **Event-Driven** via Kafka (loose coupling between services)
- **Circuit Breaker** for inter-service calls (Polly)
- **Health Checks** per service (`/health` endpoint)
- **Structured Logging** (Serilog → Console + File)
- **API Versioning** (URL-based: `/api/v1/...`)
- **Global Exception Handling** middleware
