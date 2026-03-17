using MediatR;
using TaskManager.Shared.Application.Interfaces;
using TaskManager.Shared.Application.Models;
using TaskManager.Task.Application.DTOs;
using TaskManager.Task.Domain.Entities;
using TaskManager.Task.Domain.Interfaces;

namespace TaskManager.Task.Application.Commands.CreateTask;

public class CreateTaskCommandHandler(
    ITaskItemRepository repository,
    IKafkaProducer kafkaProducer) : IRequestHandler<CreateTaskCommand, Result<TaskResponse>>
{
    public async Task<Result<TaskResponse>> Handle(CreateTaskCommand request, CancellationToken ct)
    {
        var task = new TaskItem
        {
            Title = request.Title,
            Description = request.Description,
            ProjectId = request.ProjectId,
            ReporterId = request.ReporterId,
            AssigneeId = request.AssigneeId ?? string.Empty,
            Priority = request.Priority,
            DueDate = request.DueDate,
            Tags = request.Tags ?? []
        };

        await repository.CreateAsync(task, ct);

        if (!string.IsNullOrEmpty(request.AssigneeId))
        {
            await kafkaProducer.PublishAsync("task.assigned", task.Id, new
            {
                TaskId = task.Id,
                task.Title,
                task.AssigneeId,
                task.ReporterId,
                task.ProjectId
            }, ct);
        }

        return Result<TaskResponse>.Success(MapToResponse(task), 201);
    }

    private static TaskResponse MapToResponse(TaskItem t) => new(
        t.Id, t.Title, t.Description, t.ProjectId, t.AssigneeId, t.ReporterId,
        t.Status, t.Priority, t.DueDate, t.Tags,
        t.Comments.Select(c => new TaskCommentResponse(c.Id, c.UserId, c.Content, c.CreatedAt)).ToList(),
        t.CreatedAt, t.UpdatedAt);
}
