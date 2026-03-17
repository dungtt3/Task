using MediatR;
using TaskManager.Project.Application.Commands.CreateProject;
using TaskManager.Project.Application.DTOs;
using TaskManager.Project.Domain.Interfaces;
using TaskManager.Shared.Application.Models;

namespace TaskManager.Project.Application.Commands.RemoveMember;

public class RemoveMemberCommandHandler(
    IProjectRepository repository) : IRequestHandler<RemoveMemberCommand, Result<ProjectResponse>>
{
    public async Task<Result<ProjectResponse>> Handle(RemoveMemberCommand request, CancellationToken ct)
    {
        var project = await repository.GetByIdAsync(request.ProjectId, ct);
        if (project is null)
            return Result<ProjectResponse>.Failure("Project not found", 404);

        var member = project.Members.FirstOrDefault(m => m.UserId == request.UserId);
        if (member is null)
            return Result<ProjectResponse>.Failure("User is not a member");

        project.Members.Remove(member);
        project.UpdatedAt = DateTime.UtcNow;

        await repository.UpdateAsync(project, ct);
        return Result<ProjectResponse>.Success(CreateProjectCommandHandler.MapToResponse(project));
    }
}
