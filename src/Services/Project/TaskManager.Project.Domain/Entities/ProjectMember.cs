using TaskManager.Shared.Domain.Enums;

namespace TaskManager.Project.Domain.Entities;

public class ProjectMember
{
    public string UserId { get; set; } = string.Empty;
    public ProjectRole Role { get; set; } = ProjectRole.Member;
    public DateTime JoinedAt { get; set; } = DateTime.UtcNow;
}
