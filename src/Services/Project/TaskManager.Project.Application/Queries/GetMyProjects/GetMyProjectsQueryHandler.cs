using MediatR;
using TaskManager.Project.Application.Commands.CreateProject;
using TaskManager.Project.Application.DTOs;
using TaskManager.Project.Domain.Interfaces;
using TaskManager.Shared.Application.Models;

namespace TaskManager.Project.Application.Queries.GetMyProjects;

public class GetMyProjectsQueryHandler(
    IProjectRepository repository) : IRequestHandler<GetMyProjectsQuery, Result<PagedResult<ProjectResponse>>>
{
    public async Task<Result<PagedResult<ProjectResponse>>> Handle(GetMyProjectsQuery request, CancellationToken ct)
    {
        var projects = await repository.GetByMemberIdAsync(request.UserId, ct);
        var total = projects.Count;
        var items = projects
            .Skip((request.Page - 1) * request.PageSize)
            .Take(request.PageSize)
            .Select(CreateProjectCommandHandler.MapToResponse)
            .ToList();

        return Result<PagedResult<ProjectResponse>>.Success(new PagedResult<ProjectResponse>
        {
            Items = items,
            Page = request.Page,
            PageSize = request.PageSize,
            TotalCount = total
        });
    }
}
