using MediatR;
using TaskManager.Shared.Application.Models;
using TaskManager.Task.Application.DTOs;
using TaskManager.Task.Domain.Entities;
using TaskManager.Task.Domain.Interfaces;

namespace TaskManager.Task.Application.Queries.GetTaskById;

public class GetTaskByIdQueryHandler(
    ITaskItemRepository repository) : IRequestHandler<GetTaskByIdQuery, Result<TaskResponse>>
{
    public async Task<Result<TaskResponse>> Handle(GetTaskByIdQuery request, CancellationToken ct)
    {
        var task = await repository.GetByIdAsync(request.Id, ct);
        if (task is null)
            return Result<TaskResponse>.Failure("Task not found", 404);

        return Result<TaskResponse>.Success(MapToResponse(task));
    }

    private static TaskResponse MapToResponse(TaskItem t) => new(
        t.Id, t.Title, t.Description, t.ProjectId, t.AssigneeId, t.ReporterId,
        t.Status, t.Priority, t.DueDate, t.Tags,
        t.Comments.Select(c => new TaskCommentResponse(c.Id, c.UserId, c.Content, c.CreatedAt)).ToList(),
        t.CreatedAt, t.UpdatedAt);
}
