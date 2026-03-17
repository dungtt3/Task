using MediatR;
using Microsoft.Extensions.Logging;
using TaskManager.Project.Application.DTOs;
using TaskManager.Project.Domain.Interfaces;
using TaskManager.Shared.Application.Exceptions;

namespace TaskManager.Project.Application.Commands.UpdateMemberRole;

public class UpdateMemberRoleCommandHandler : IRequestHandler<UpdateMemberRoleCommand, ProjectResponse>
{
    private readonly IProjectRepository _projectRepository;
    private readonly ILogger<UpdateMemberRoleCommandHandler> _logger;

    public UpdateMemberRoleCommandHandler(
        IProjectRepository projectRepository,
        ILogger<UpdateMemberRoleCommandHandler> logger)
    {
        _projectRepository = projectRepository;
        _logger = logger;
    }

    public async Task<ProjectResponse> Handle(UpdateMemberRoleCommand request, CancellationToken ct)
    {
        var project = await _projectRepository.GetByIdAsync(request.ProjectId, ct)
            ?? throw new NotFoundException("Project", request.ProjectId);

        var member = project.Members.FirstOrDefault(m => m.UserId == request.UserId)
            ?? throw new NotFoundException("ProjectMember", request.UserId);

        member.Role = request.Role;
        project.UpdatedAt = DateTime.UtcNow;

        await _projectRepository.UpdateAsync(project, ct);

        _logger.LogInformation("Member {UserId} role updated to {Role} in project {ProjectId}",
            request.UserId, request.Role, project.Id);

        return MapToResponse(project);
    }

    private static ProjectResponse MapToResponse(Domain.Entities.Project project) => new(
        project.Id,
        project.Name,
        project.Description,
        project.OwnerId,
        project.Members.Select(m => new ProjectMemberResponse(m.UserId, m.Role, m.JoinedAt)).ToList(),
        project.IsArchived,
        project.CreatedAt,
        project.UpdatedAt);
}
