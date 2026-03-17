using System.Security.Claims;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using TaskManager.Project.Application.Commands.AddMember;
using TaskManager.Project.Application.Commands.CreateProject;
using TaskManager.Project.Application.Commands.DeleteProject;
using TaskManager.Project.Application.Commands.RemoveMember;
using TaskManager.Project.Application.Commands.UpdateProject;
using TaskManager.Project.Application.DTOs;
using TaskManager.Project.Application.Queries.GetMyProjects;
using TaskManager.Project.Application.Queries.GetProjectById;
using TaskManager.Shared.Domain.Enums;

namespace TaskManager.Project.API.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class ProjectsController(IMediator mediator) : ControllerBase
{
    [HttpPost]
    public async Task<IActionResult> Create([FromBody] CreateProjectRequest request)
    {
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier)!;
        var result = await mediator.Send(new CreateProjectCommand(request.Name, request.Description, userId));
        return result.IsSuccess ? StatusCode(result.StatusCode, result.Data) : StatusCode(result.StatusCode, result.Error);
    }

    [HttpGet("{id}")]
    public async Task<IActionResult> GetById(string id)
    {
        var result = await mediator.Send(new GetProjectByIdQuery(id));
        return result.IsSuccess ? Ok(result.Data) : StatusCode(result.StatusCode, result.Error);
    }

    [HttpGet("my")]
    public async Task<IActionResult> GetMyProjects([FromQuery] int page = 1, [FromQuery] int pageSize = 20)
    {
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier)!;
        var result = await mediator.Send(new GetMyProjectsQuery(userId, page, pageSize));
        return result.IsSuccess ? Ok(result.Data) : StatusCode(result.StatusCode, result.Error);
    }

    [HttpPut("{id}")]
    public async Task<IActionResult> Update(string id, [FromBody] UpdateProjectRequest request)
    {
        var result = await mediator.Send(new UpdateProjectCommand(id, request.Name, request.Description, request.IsArchived));
        return result.IsSuccess ? Ok(result.Data) : StatusCode(result.StatusCode, result.Error);
    }

    [HttpPost("{id}/members")]
    public async Task<IActionResult> AddMember(string id, [FromBody] AddMemberRequest request)
    {
        var result = await mediator.Send(new AddMemberCommand(id, request.UserId, request.Role));
        return result.IsSuccess ? Ok(result.Data) : StatusCode(result.StatusCode, result.Error);
    }

    [HttpDelete("{id}/members/{userId}")]
    public async Task<IActionResult> RemoveMember(string id, string userId)
    {
        var result = await mediator.Send(new RemoveMemberCommand(id, userId));
        return result.IsSuccess ? Ok(result.Data) : StatusCode(result.StatusCode, result.Error);
    }

    [HttpDelete("{id}")]
    public async Task<IActionResult> Delete(string id)
    {
        var result = await mediator.Send(new DeleteProjectCommand(id));
        return result.IsSuccess ? NoContent() : StatusCode(result.StatusCode, result.Error);
    }
}

public record AddMemberRequest(string UserId, ProjectRole Role = ProjectRole.Member);
