using MediatR;
using TaskManager.Shared.Application.Models;
using TaskManager.Task.Domain.Interfaces;

namespace TaskManager.Task.Application.Commands.DeleteTask;

public class DeleteTaskCommandHandler(
    ITaskItemRepository repository) : IRequestHandler<DeleteTaskCommand, Result<bool>>
{
    public async Task<Result<bool>> Handle(DeleteTaskCommand request, CancellationToken ct)
    {
        var task = await repository.GetByIdAsync(request.Id, ct);
        if (task is null)
            return Result<bool>.Failure("Task not found", 404);

        await repository.DeleteAsync(request.Id, ct);
        return Result<bool>.Success(true);
    }
}
