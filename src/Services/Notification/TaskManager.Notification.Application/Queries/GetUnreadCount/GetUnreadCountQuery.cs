using MediatR;
using TaskManager.Shared.Application.Models;

namespace TaskManager.Notification.Application.Queries.GetUnreadCount;

public record GetUnreadCountQuery(string UserId) : IRequest<Result<long>>;
