// ─── Enums (const objects for erasableSyntaxOnly) ───────
export const Priority = {
  Low: 0,
  Medium: 1,
  High: 2,
  Urgent: 3,
} as const;
export type Priority = (typeof Priority)[keyof typeof Priority];

export const TaskItemStatus = {
  Todo: 0,
  InProgress: 1,
  Review: 2,
  Done: 3,
} as const;
export type TaskItemStatus = (typeof TaskItemStatus)[keyof typeof TaskItemStatus];

export const ProjectRole = {
  Member: 0,
  Admin: 1,
  Owner: 2,
} as const;
export type ProjectRole = (typeof ProjectRole)[keyof typeof ProjectRole];

export const NotificationType = {
  TaskAssigned: 0,
  TaskCompleted: 1,
  DueReminder: 2,
  ProjectUpdate: 3,
} as const;
export type NotificationType = (typeof NotificationType)[keyof typeof NotificationType];

export const ReferenceType = {
  Task: 0,
  Project: 1,
} as const;
export type ReferenceType = (typeof ReferenceType)[keyof typeof ReferenceType];

// ─── Auth DTOs ──────────────────────────────────────────
export interface AuthResponse {
  id: string;
  email: string;
  displayName: string;
  accessToken: string;
  refreshToken: string;
}

export interface UserDto {
  id: string;
  email: string;
  displayName: string;
  avatar?: string;
}

export interface LoginRequest {
  email: string;
  password: string;
}

export interface RegisterRequest {
  email: string;
  password: string;
  displayName: string;
}

// ─── Task DTOs ──────────────────────────────────────────
export interface TaskResponse {
  id: string;
  title: string;
  description: string;
  projectId: string;
  assigneeId: string;
  reporterId: string;
  status: TaskItemStatus;
  priority: Priority;
  dueDate?: string;
  tags: string[];
  comments: TaskCommentResponse[];
  createdAt: string;
  updatedAt: string;
}

export interface TaskCommentResponse {
  id: string;
  userId: string;
  content: string;
  createdAt: string;
}

export interface CreateTaskRequest {
  title: string;
  description: string;
  projectId: string;
  assigneeId?: string;
  priority?: Priority;
  dueDate?: string;
  tags?: string[];
}

export interface UpdateTaskRequest {
  title?: string;
  description?: string;
  assigneeId?: string;
  priority?: Priority;
  dueDate?: string;
  tags?: string[];
}

// ─── Project DTOs ───────────────────────────────────────
export interface ProjectResponse {
  id: string;
  name: string;
  description: string;
  ownerId: string;
  members: ProjectMemberResponse[];
  isArchived: boolean;
  createdAt: string;
  updatedAt: string;
}

export interface ProjectMemberResponse {
  userId: string;
  role: ProjectRole;
  joinedAt: string;
}

export interface CreateProjectRequest {
  name: string;
  description: string;
}

export interface UpdateProjectRequest {
  name?: string;
  description?: string;
  isArchived?: boolean;
}

// ─── Notification DTOs ──────────────────────────────────
export interface NotificationResponse {
  id: string;
  userId: string;
  title: string;
  message: string;
  type: NotificationType;
  referenceId: string;
  referenceType: ReferenceType;
  isRead: boolean;
  readAt?: string;
  createdAt: string;
}

// ─── Pagination ─────────────────────────────────────────
export interface PagedResult<T> {
  items: T[];
  totalCount: number;
  page: number;
  pageSize: number;
  totalPages: number;
}

// ─── API Error ──────────────────────────────────────────
export interface ApiError {
  type: string;
  title: string;
  status: number;
  detail?: string;
  errors?: Record<string, string[]>;
}
