using MediatR;
using TaskManager.Shared.Application.Models;
using TaskManager.Task.Application.DTOs;
using TaskManager.Task.Domain.Entities;
using TaskManager.Task.Domain.Interfaces;

namespace TaskManager.Task.Application.Queries.GetTasksByProject;

public class GetTasksByProjectQueryHandler(
    ITaskItemRepository repository) : IRequestHandler<GetTasksByProjectQuery, Result<PagedResult<TaskResponse>>>
{
    public async Task<Result<PagedResult<TaskResponse>>> Handle(GetTasksByProjectQuery request, CancellationToken ct)
    {
        var tasks = await repository.GetByProjectIdAsync(request.ProjectId, ct);
        var total = tasks.Count;
        var items = tasks
            .Skip((request.Page - 1) * request.PageSize)
            .Take(request.PageSize)
            .Select(MapToResponse)
            .ToList();

        var result = new PagedResult<TaskResponse>
        {
            Items = items,
            Page = request.Page,
            PageSize = request.PageSize,
            TotalCount = total
        };

        return Result<PagedResult<TaskResponse>>.Success(result);
    }

    private static TaskResponse MapToResponse(TaskItem t) => new(
        t.Id, t.Title, t.Description, t.ProjectId, t.AssigneeId, t.ReporterId,
        t.Status, t.Priority, t.DueDate, t.Tags,
        t.Comments.Select(c => new TaskCommentResponse(c.Id, c.UserId, c.Content, c.CreatedAt)).ToList(),
        t.CreatedAt, t.UpdatedAt);
}
