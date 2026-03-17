using MediatR;
using TaskManager.Shared.Application.Models;
using TaskManager.Shared.Domain.Enums;
using TaskManager.Task.Application.DTOs;

namespace TaskManager.Task.Application.Commands.UpdateTask;

public record UpdateTaskCommand(
    string Id,
    string? Title,
    string? Description,
    string? AssigneeId,
    Priority? Priority,
    DateTime? DueDate,
    List<string>? Tags) : IRequest<Result<TaskResponse>>;
