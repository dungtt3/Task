using MediatR;
using TaskManager.Project.Domain.Interfaces;
using TaskManager.Shared.Application.Models;

namespace TaskManager.Project.Application.Commands.DeleteProject;

public class DeleteProjectCommandHandler(
    IProjectRepository repository) : IRequestHandler<DeleteProjectCommand, Result<bool>>
{
    public async Task<Result<bool>> Handle(DeleteProjectCommand request, CancellationToken ct)
    {
        var project = await repository.GetByIdAsync(request.Id, ct);
        if (project is null)
            return Result<bool>.Failure("Project not found", 404);

        await repository.DeleteAsync(request.Id, ct);
        return Result<bool>.Success(true);
    }
}
