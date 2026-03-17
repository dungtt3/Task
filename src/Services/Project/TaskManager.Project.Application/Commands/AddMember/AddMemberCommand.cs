using MediatR;
using TaskManager.Project.Application.DTOs;
using TaskManager.Shared.Application.Models;
using TaskManager.Shared.Domain.Enums;

namespace TaskManager.Project.Application.Commands.AddMember;

public record AddMemberCommand(string ProjectId, string UserId, ProjectRole Role = ProjectRole.Member) : IRequest<Result<ProjectResponse>>;
