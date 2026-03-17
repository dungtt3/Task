using MediatR;
using TaskManager.Project.Application.DTOs;

namespace TaskManager.Project.Application.Queries.GetProjectMembers;

public record GetProjectMembersQuery(string ProjectId) : IRequest<IReadOnlyList<ProjectMemberResponse>>;
