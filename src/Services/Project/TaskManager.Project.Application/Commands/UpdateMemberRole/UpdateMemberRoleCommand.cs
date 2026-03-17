using MediatR;
using TaskManager.Project.Application.DTOs;
using TaskManager.Shared.Domain.Enums;

namespace TaskManager.Project.Application.Commands.UpdateMemberRole;

public record UpdateMemberRoleCommand(
    string ProjectId,
    string UserId,
    ProjectRole Role) : IRequest<ProjectResponse>;
