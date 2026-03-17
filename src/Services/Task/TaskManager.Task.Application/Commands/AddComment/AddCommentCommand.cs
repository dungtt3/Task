using MediatR;
using TaskManager.Shared.Application.Models;
using TaskManager.Task.Application.DTOs;

namespace TaskManager.Task.Application.Commands.AddComment;

public record AddCommentCommand(string TaskId, string UserId, string Content) : IRequest<Result<TaskResponse>>;
