using System.Security.Claims;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using TaskManager.Shared.Domain.Enums;
using TaskManager.Task.Application.Commands.AddComment;
using TaskManager.Task.Application.Commands.CreateTask;
using TaskManager.Task.Application.Commands.DeleteTask;
using TaskManager.Task.Application.Commands.UpdateTask;
using TaskManager.Task.Application.Commands.UpdateTaskStatus;
using TaskManager.Task.Application.DTOs;
using TaskManager.Task.Application.Queries.GetTaskById;
using TaskManager.Task.Application.Queries.GetTasksByAssignee;
using TaskManager.Task.Application.Queries.GetTasksByProject;

namespace TaskManager.Task.API.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class TasksController(IMediator mediator) : ControllerBase
{
    [HttpPost]
    public async Task<IActionResult> Create([FromBody] CreateTaskRequest request)
    {
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier)!;
        var command = new CreateTaskCommand(
            request.Title, request.Description, request.ProjectId, userId,
            request.AssigneeId, request.Priority, request.DueDate, request.Tags);
        var result = await mediator.Send(command);
        return result.IsSuccess ? StatusCode(result.StatusCode, result.Data) : StatusCode(result.StatusCode, result.Error);
    }

    [HttpGet("{id}")]
    public async Task<IActionResult> GetById(string id)
    {
        var result = await mediator.Send(new GetTaskByIdQuery(id));
        return result.IsSuccess ? Ok(result.Data) : StatusCode(result.StatusCode, result.Error);
    }

    [HttpGet("project/{projectId}")]
    public async Task<IActionResult> GetByProject(string projectId, [FromQuery] int page = 1, [FromQuery] int pageSize = 20)
    {
        var result = await mediator.Send(new GetTasksByProjectQuery(projectId, page, pageSize));
        return result.IsSuccess ? Ok(result.Data) : StatusCode(result.StatusCode, result.Error);
    }

    [HttpGet("my")]
    public async Task<IActionResult> GetMyTasks()
    {
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier)!;
        var result = await mediator.Send(new GetTasksByAssigneeQuery(userId));
        return result.IsSuccess ? Ok(result.Data) : StatusCode(result.StatusCode, result.Error);
    }

    [HttpPut("{id}")]
    public async Task<IActionResult> Update(string id, [FromBody] UpdateTaskRequest request)
    {
        var command = new UpdateTaskCommand(id, request.Title, request.Description,
            request.AssigneeId, request.Priority, request.DueDate, request.Tags);
        var result = await mediator.Send(command);
        return result.IsSuccess ? Ok(result.Data) : StatusCode(result.StatusCode, result.Error);
    }

    [HttpPatch("{id}/status")]
    public async Task<IActionResult> UpdateStatus(string id, [FromBody] TaskItemStatus status)
    {
        var result = await mediator.Send(new UpdateTaskStatusCommand(id, status));
        return result.IsSuccess ? Ok(result.Data) : StatusCode(result.StatusCode, result.Error);
    }

    [HttpPost("{id}/comments")]
    public async Task<IActionResult> AddComment(string id, [FromBody] AddCommentRequest request)
    {
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier)!;
        var result = await mediator.Send(new AddCommentCommand(id, userId, request.Content));
        return result.IsSuccess ? Ok(result.Data) : StatusCode(result.StatusCode, result.Error);
    }

    [HttpDelete("{id}")]
    public async Task<IActionResult> Delete(string id)
    {
        var result = await mediator.Send(new DeleteTaskCommand(id));
        return result.IsSuccess ? NoContent() : StatusCode(result.StatusCode, result.Error);
    }
}

public record AddCommentRequest(string Content);
