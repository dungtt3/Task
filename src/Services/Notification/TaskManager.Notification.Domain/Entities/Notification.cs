using TaskManager.Shared.Domain.Entities;
using TaskManager.Shared.Domain.Enums;

namespace TaskManager.Notification.Domain.Entities;

public class Notification : AuditableEntity
{
    public string UserId { get; set; } = string.Empty;
    public string Title { get; set; } = string.Empty;
    public string Message { get; set; } = string.Empty;
    public NotificationType Type { get; set; }
    public string ReferenceId { get; set; } = string.Empty;
    public ReferenceType ReferenceType { get; set; }
    public bool IsRead { get; set; }
    public DateTime? ReadAt { get; set; }
}
