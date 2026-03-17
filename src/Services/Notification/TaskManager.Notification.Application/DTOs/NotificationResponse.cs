using TaskManager.Shared.Domain.Enums;

namespace TaskManager.Notification.Application.DTOs;

public record NotificationResponse(
    string Id,
    string UserId,
    string Title,
    string Message,
    NotificationType Type,
    string ReferenceId,
    ReferenceType ReferenceType,
    bool IsRead,
    DateTime? ReadAt,
    DateTime CreatedAt);
