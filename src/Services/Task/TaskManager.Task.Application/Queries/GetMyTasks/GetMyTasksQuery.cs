using MediatR;
using TaskManager.Task.Application.DTOs;

namespace TaskManager.Task.Application.Queries.GetMyTasks;

public record GetMyTasksQuery(string UserId) : IRequest<IReadOnlyList<TaskResponse>>;
