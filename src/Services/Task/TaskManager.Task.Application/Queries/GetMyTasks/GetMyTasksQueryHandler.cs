using MediatR;
using TaskManager.Task.Application.DTOs;
using TaskManager.Task.Domain.Entities;
using TaskManager.Task.Domain.Interfaces;

namespace TaskManager.Task.Application.Queries.GetMyTasks;

public class GetMyTasksQueryHandler : IRequestHandler<GetMyTasksQuery, IReadOnlyList<TaskResponse>>
{
    private readonly ITaskItemRepository _taskRepository;

    public GetMyTasksQueryHandler(ITaskItemRepository taskRepository)
    {
        _taskRepository = taskRepository;
    }

    public async Task<IReadOnlyList<TaskResponse>> Handle(GetMyTasksQuery request, CancellationToken ct)
    {
        var tasks = await _taskRepository.GetByAssigneeIdAsync(request.UserId, ct);

        return tasks.Select(MapToResponse).ToList();
    }

    private static TaskResponse MapToResponse(TaskItem task) => new(
        task.Id,
        task.Title,
        task.Description,
        task.ProjectId,
        task.AssigneeId,
        task.ReporterId,
        task.Status,
        task.Priority,
        task.DueDate,
        task.Tags,
        task.Comments.Select(c => new TaskCommentResponse(c.Id, c.UserId, c.Content, c.CreatedAt)).ToList(),
        task.CreatedAt,
        task.UpdatedAt);
}
