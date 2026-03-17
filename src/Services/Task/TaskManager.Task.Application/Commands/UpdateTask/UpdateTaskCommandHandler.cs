using MediatR;
using TaskManager.Shared.Application.Models;
using TaskManager.Task.Application.DTOs;
using TaskManager.Task.Domain.Interfaces;

namespace TaskManager.Task.Application.Commands.UpdateTask;

public class UpdateTaskCommandHandler(
    ITaskItemRepository repository) : IRequestHandler<UpdateTaskCommand, Result<TaskResponse>>
{
    public async Task<Result<TaskResponse>> Handle(UpdateTaskCommand request, CancellationToken ct)
    {
        var task = await repository.GetByIdAsync(request.Id, ct);
        if (task is null)
            return Result<TaskResponse>.Failure("Task not found", 404);

        if (request.Title is not null) task.Title = request.Title;
        if (request.Description is not null) task.Description = request.Description;
        if (request.AssigneeId is not null) task.AssigneeId = request.AssigneeId;
        if (request.Priority.HasValue) task.Priority = request.Priority.Value;
        if (request.DueDate.HasValue) task.DueDate = request.DueDate.Value;
        if (request.Tags is not null) task.Tags = request.Tags;
        task.UpdatedAt = DateTime.UtcNow;

        await repository.UpdateAsync(task, ct);

        return Result<TaskResponse>.Success(MapToResponse(task));
    }

    private static TaskResponse MapToResponse(Domain.Entities.TaskItem t) => new(
        t.Id, t.Title, t.Description, t.ProjectId, t.AssigneeId, t.ReporterId,
        t.Status, t.Priority, t.DueDate, t.Tags,
        t.Comments.Select(c => new TaskCommentResponse(c.Id, c.UserId, c.Content, c.CreatedAt)).ToList(),
        t.CreatedAt, t.UpdatedAt);
}
