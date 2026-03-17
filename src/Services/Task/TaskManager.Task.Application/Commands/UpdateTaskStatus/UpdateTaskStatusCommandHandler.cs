using MediatR;
using TaskManager.Shared.Application.Interfaces;
using TaskManager.Shared.Application.Models;
using TaskManager.Shared.Domain.Enums;
using TaskManager.Task.Application.DTOs;
using TaskManager.Task.Domain.Interfaces;

namespace TaskManager.Task.Application.Commands.UpdateTaskStatus;

public class UpdateTaskStatusCommandHandler(
    ITaskItemRepository repository,
    IKafkaProducer kafkaProducer) : IRequestHandler<UpdateTaskStatusCommand, Result<TaskResponse>>
{
    public async Task<Result<TaskResponse>> Handle(UpdateTaskStatusCommand request, CancellationToken ct)
    {
        var task = await repository.GetByIdAsync(request.Id, ct);
        if (task is null)
            return Result<TaskResponse>.Failure("Task not found", 404);

        task.Status = request.Status;
        task.UpdatedAt = DateTime.UtcNow;
        await repository.UpdateAsync(task, ct);

        var topic = request.Status == TaskItemStatus.Done ? "task.completed" : "task.status-changed";
        await kafkaProducer.PublishAsync(topic, task.Id, new
        {
            TaskId = task.Id,
            task.Title,
            task.AssigneeId,
            task.ProjectId,
            NewStatus = request.Status.ToString()
        }, ct);

        return Result<TaskResponse>.Success(MapToResponse(task));
    }

    private static TaskResponse MapToResponse(Domain.Entities.TaskItem t) => new(
        t.Id, t.Title, t.Description, t.ProjectId, t.AssigneeId, t.ReporterId,
        t.Status, t.Priority, t.DueDate, t.Tags,
        t.Comments.Select(c => new TaskCommentResponse(c.Id, c.UserId, c.Content, c.CreatedAt)).ToList(),
        t.CreatedAt, t.UpdatedAt);
}
