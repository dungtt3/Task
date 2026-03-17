using MediatR;
using TaskManager.Shared.Application.Models;
using TaskManager.Task.Application.DTOs;
using TaskManager.Task.Domain.Entities;
using TaskManager.Task.Domain.Interfaces;

namespace TaskManager.Task.Application.Queries.GetTasksByAssignee;

public class GetTasksByAssigneeQueryHandler(
    ITaskItemRepository repository) : IRequestHandler<GetTasksByAssigneeQuery, Result<IReadOnlyList<TaskResponse>>>
{
    public async Task<Result<IReadOnlyList<TaskResponse>>> Handle(GetTasksByAssigneeQuery request, CancellationToken ct)
    {
        var tasks = await repository.GetByAssigneeIdAsync(request.AssigneeId, ct);
        var items = tasks.Select(MapToResponse).ToList() as IReadOnlyList<TaskResponse>;
        return Result<IReadOnlyList<TaskResponse>>.Success(items);
    }

    private static TaskResponse MapToResponse(TaskItem t) => new(
        t.Id, t.Title, t.Description, t.ProjectId, t.AssigneeId, t.ReporterId,
        t.Status, t.Priority, t.DueDate, t.Tags,
        t.Comments.Select(c => new TaskCommentResponse(c.Id, c.UserId, c.Content, c.CreatedAt)).ToList(),
        t.CreatedAt, t.UpdatedAt);
}
