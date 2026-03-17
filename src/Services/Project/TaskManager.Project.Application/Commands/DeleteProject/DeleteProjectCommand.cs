using MediatR;
using TaskManager.Shared.Application.Models;

namespace TaskManager.Project.Application.Commands.DeleteProject;

public record DeleteProjectCommand(string Id) : IRequest<Result<bool>>;
