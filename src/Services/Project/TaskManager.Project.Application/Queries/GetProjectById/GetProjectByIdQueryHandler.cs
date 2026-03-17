using MediatR;
using TaskManager.Project.Application.Commands.CreateProject;
using TaskManager.Project.Application.DTOs;
using TaskManager.Project.Domain.Interfaces;
using TaskManager.Shared.Application.Models;

namespace TaskManager.Project.Application.Queries.GetProjectById;

public class GetProjectByIdQueryHandler(
    IProjectRepository repository) : IRequestHandler<GetProjectByIdQuery, Result<ProjectResponse>>
{
    public async Task<Result<ProjectResponse>> Handle(GetProjectByIdQuery request, CancellationToken ct)
    {
        var project = await repository.GetByIdAsync(request.Id, ct);
        if (project is null)
            return Result<ProjectResponse>.Failure("Project not found", 404);

        return Result<ProjectResponse>.Success(CreateProjectCommandHandler.MapToResponse(project));
    }
}
