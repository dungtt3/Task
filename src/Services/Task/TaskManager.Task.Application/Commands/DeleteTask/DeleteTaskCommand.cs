using MediatR;
using TaskManager.Shared.Application.Models;

namespace TaskManager.Task.Application.Commands.DeleteTask;

public record DeleteTaskCommand(string Id) : IRequest<Result<bool>>;
