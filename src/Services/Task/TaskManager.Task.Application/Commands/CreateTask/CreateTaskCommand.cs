using MediatR;
using TaskManager.Shared.Application.Models;
using TaskManager.Shared.Domain.Enums;
using TaskManager.Task.Application.DTOs;

namespace TaskManager.Task.Application.Commands.CreateTask;

public record CreateTaskCommand(
    string Title,
    string Description,
    string ProjectId,
    string ReporterId,
    string? AssigneeId,
    Priority Priority = Priority.Medium,
    DateTime? DueDate = null,
    List<string>? Tags = null) : IRequest<Result<TaskResponse>>;
