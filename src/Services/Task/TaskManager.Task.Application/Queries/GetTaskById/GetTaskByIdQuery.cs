using MediatR;
using TaskManager.Shared.Application.Models;
using TaskManager.Task.Application.DTOs;

namespace TaskManager.Task.Application.Queries.GetTaskById;

public record GetTaskByIdQuery(string Id) : IRequest<Result<TaskResponse>>;
