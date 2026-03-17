using TaskManager.Shared.Domain.Entities;
using TaskManager.Shared.Domain.Enums;

namespace TaskManager.Task.Domain.Entities;

public class TaskItem : AuditableEntity
{
    public string Title { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public string ProjectId { get; set; } = string.Empty;
    public string AssigneeId { get; set; } = string.Empty;
    public string ReporterId { get; set; } = string.Empty;
    public TaskItemStatus Status { get; set; } = TaskItemStatus.Todo;
    public Priority Priority { get; set; } = Priority.Medium;
    public DateTime? DueDate { get; set; }
    public List<string> Tags { get; set; } = [];
    public List<TaskComment> Comments { get; set; } = [];
}
