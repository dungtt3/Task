using MediatR;
using TaskManager.Project.Application.DTOs;
using TaskManager.Project.Domain.Entities;
using TaskManager.Project.Domain.Interfaces;
using TaskManager.Shared.Application.Models;
using TaskManager.Shared.Domain.Enums;

namespace TaskManager.Project.Application.Commands.CreateProject;

public class CreateProjectCommandHandler(
    IProjectRepository repository) : IRequestHandler<CreateProjectCommand, Result<ProjectResponse>>
{
    public async Task<Result<ProjectResponse>> Handle(CreateProjectCommand request, CancellationToken ct)
    {
        var project = new Domain.Entities.Project
        {
            Name = request.Name,
            Description = request.Description,
            OwnerId = request.OwnerId,
            Members = [new ProjectMember { UserId = request.OwnerId, Role = ProjectRole.Owner }]
        };

        await repository.CreateAsync(project, ct);
        return Result<ProjectResponse>.Success(MapToResponse(project), 201);
    }

    internal static ProjectResponse MapToResponse(Domain.Entities.Project p) => new(
        p.Id, p.Name, p.Description, p.OwnerId,
        p.Members.Select(m => new ProjectMemberResponse(m.UserId, m.Role, m.JoinedAt)).ToList(),
        p.IsArchived, p.CreatedAt, p.UpdatedAt);
}
