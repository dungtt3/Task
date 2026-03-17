using MediatR;
using TaskManager.Notification.Application.DTOs;
using TaskManager.Shared.Application.Models;
using TaskManager.Shared.Domain.Enums;

namespace TaskManager.Notification.Application.Commands.CreateNotification;

public record CreateNotificationCommand(
    string UserId,
    string Title,
    string Message,
    NotificationType Type,
    string ReferenceId,
    ReferenceType ReferenceType) : IRequest<Result<NotificationResponse>>;
