using MediatR;
using TaskManager.Project.Application.DTOs;
using TaskManager.Shared.Application.Models;

namespace TaskManager.Project.Application.Commands.RemoveMember;

public record RemoveMemberCommand(string ProjectId, string UserId) : IRequest<Result<ProjectResponse>>;
