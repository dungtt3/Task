using MediatR;
using TaskManager.Project.Application.DTOs;
using TaskManager.Shared.Application.Models;

namespace TaskManager.Project.Application.Commands.UpdateProject;

public record UpdateProjectCommand(string Id, string? Name, string? Description, bool? IsArchived) : IRequest<Result<ProjectResponse>>;
