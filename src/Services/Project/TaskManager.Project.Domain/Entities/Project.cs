using TaskManager.Shared.Domain.Entities;

namespace TaskManager.Project.Domain.Entities;

public class Project : AuditableEntity
{
    public string Name { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public string OwnerId { get; set; } = string.Empty;
    public List<ProjectMember> Members { get; set; } = [];
    public bool IsArchived { get; set; }
}
