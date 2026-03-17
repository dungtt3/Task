using TaskManager.Shared.Domain.Enums;

namespace TaskManager.Task.Application.DTOs;

public record TaskResponse(
    string Id,
    string Title,
    string Description,
    string ProjectId,
    string AssigneeId,
    string ReporterId,
    TaskItemStatus Status,
    Priority Priority,
    DateTime? DueDate,
    List<string> Tags,
    List<TaskCommentResponse> Comments,
    DateTime CreatedAt,
    DateTime UpdatedAt);

public record TaskCommentResponse(
    string Id,
    string UserId,
    string Content,
    DateTime CreatedAt);

public record CreateTaskRequest(
    string Title,
    string Description,
    string ProjectId,
    string? AssigneeId,
    Priority Priority = Priority.Medium,
    DateTime? DueDate = null,
    List<string>? Tags = null);

public record UpdateTaskRequest(
    string? Title,
    string? Description,
    string? AssigneeId,
    Priority? Priority,
    DateTime? DueDate,
    List<string>? Tags);
