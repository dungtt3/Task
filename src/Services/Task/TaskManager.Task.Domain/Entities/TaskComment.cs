using MongoDB.Bson;

namespace TaskManager.Task.Domain.Entities;

public class TaskComment
{
    public string Id { get; set; } = ObjectId.GenerateNewId().ToString();
    public string UserId { get; set; } = string.Empty;
    public string Content { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
}
