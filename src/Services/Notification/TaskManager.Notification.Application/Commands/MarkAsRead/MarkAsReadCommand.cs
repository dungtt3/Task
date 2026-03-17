using MediatR;
using TaskManager.Shared.Application.Models;

namespace TaskManager.Notification.Application.Commands.MarkAsRead;

public record MarkAsReadCommand(string Id) : IRequest<Result<bool>>;
