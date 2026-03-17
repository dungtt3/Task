using MediatR;
using TaskManager.Shared.Application.Models;
using TaskManager.Task.Application.DTOs;

namespace TaskManager.Task.Application.Queries.GetTasksByAssignee;

public record GetTasksByAssigneeQuery(string AssigneeId) : IRequest<Result<IReadOnlyList<TaskResponse>>>;
