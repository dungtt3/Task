using MediatR;
using TaskManager.Shared.Application.Models;

namespace TaskManager.Notification.Application.Commands.MarkAllAsRead;

public record MarkAllAsReadCommand(string UserId) : IRequest<Result<bool>>;
