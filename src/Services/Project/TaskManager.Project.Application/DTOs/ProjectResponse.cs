using TaskManager.Shared.Domain.Enums;

namespace TaskManager.Project.Application.DTOs;

public record ProjectResponse(
    string Id,
    string Name,
    string Description,
    string OwnerId,
    List<ProjectMemberResponse> Members,
    bool IsArchived,
    DateTime CreatedAt,
    DateTime UpdatedAt);

public record ProjectMemberResponse(string UserId, ProjectRole Role, DateTime JoinedAt);

public record CreateProjectRequest(string Name, string Description);

public record UpdateProjectRequest(string? Name, string? Description, bool? IsArchived);
