using MediatR;
using TaskManager.Project.Application.DTOs;
using TaskManager.Shared.Application.Models;

namespace TaskManager.Project.Application.Commands.CreateProject;

public record CreateProjectCommand(string Name, string Description, string OwnerId) : IRequest<Result<ProjectResponse>>;
