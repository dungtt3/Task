using MediatR;
using TaskManager.Shared.Application.Models;
using TaskManager.Shared.Domain.Enums;
using TaskManager.Task.Application.DTOs;

namespace TaskManager.Task.Application.Commands.UpdateTaskStatus;

public record UpdateTaskStatusCommand(string Id, TaskItemStatus Status) : IRequest<Result<TaskResponse>>;
