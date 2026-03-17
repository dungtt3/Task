using MediatR;
using TaskManager.Project.Application.DTOs;
using TaskManager.Shared.Application.Models;

namespace TaskManager.Project.Application.Queries.GetMyProjects;

public record GetMyProjectsQuery(string UserId, int Page = 1, int PageSize = 20) : IRequest<Result<PagedResult<ProjectResponse>>>;
