using MediatR;
using TaskManager.Project.Application.Commands.CreateProject;
using TaskManager.Project.Application.DTOs;
using TaskManager.Project.Domain.Entities;
using TaskManager.Project.Domain.Interfaces;
using TaskManager.Shared.Application.Interfaces;
using TaskManager.Shared.Application.Models;

namespace TaskManager.Project.Application.Commands.AddMember;

public class AddMemberCommandHandler(
    IProjectRepository repository,
    IKafkaProducer kafkaProducer) : IRequestHandler<AddMemberCommand, Result<ProjectResponse>>
{
    public async Task<Result<ProjectResponse>> Handle(AddMemberCommand request, CancellationToken ct)
    {
        var project = await repository.GetByIdAsync(request.ProjectId, ct);
        if (project is null)
            return Result<ProjectResponse>.Failure("Project not found", 404);

        if (project.Members.Any(m => m.UserId == request.UserId))
            return Result<ProjectResponse>.Failure("User is already a member");

        project.Members.Add(new ProjectMember
        {
            UserId = request.UserId,
            Role = request.Role
        });
        project.UpdatedAt = DateTime.UtcNow;

        await repository.UpdateAsync(project, ct);

        await kafkaProducer.PublishAsync("project.updated", project.Id, new
        {
            project.Id,
            project.Name,
            Action = "member_added",
            MemberUserId = request.UserId
        }, ct);

        return Result<ProjectResponse>.Success(CreateProjectCommandHandler.MapToResponse(project));
    }
}
