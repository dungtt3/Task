using MediatR;
using TaskManager.Project.Application.Commands.CreateProject;
using TaskManager.Project.Application.DTOs;
using TaskManager.Project.Domain.Interfaces;
using TaskManager.Shared.Application.Models;

namespace TaskManager.Project.Application.Commands.UpdateProject;

public class UpdateProjectCommandHandler(
    IProjectRepository repository) : IRequestHandler<UpdateProjectCommand, Result<ProjectResponse>>
{
    public async Task<Result<ProjectResponse>> Handle(UpdateProjectCommand request, CancellationToken ct)
    {
        var project = await repository.GetByIdAsync(request.Id, ct);
        if (project is null)
            return Result<ProjectResponse>.Failure("Project not found", 404);

        if (request.Name is not null) project.Name = request.Name;
        if (request.Description is not null) project.Description = request.Description;
        if (request.IsArchived.HasValue) project.IsArchived = request.IsArchived.Value;
        project.UpdatedAt = DateTime.UtcNow;

        await repository.UpdateAsync(project, ct);
        return Result<ProjectResponse>.Success(CreateProjectCommandHandler.MapToResponse(project));
    }
}
