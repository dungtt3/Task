using MediatR;
using TaskManager.Project.Application.DTOs;
using TaskManager.Shared.Application.Models;

namespace TaskManager.Project.Application.Queries.GetProjectById;

public record GetProjectByIdQuery(string Id) : IRequest<Result<ProjectResponse>>;
