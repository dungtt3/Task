using MediatR;
using TaskManager.Shared.Application.Models;
using TaskManager.Task.Application.DTOs;

namespace TaskManager.Task.Application.Queries.GetTasksByProject;

public record GetTasksByProjectQuery(string ProjectId, int Page = 1, int PageSize = 20) : IRequest<Result<PagedResult<TaskResponse>>>;
