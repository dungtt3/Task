using MediatR;
using TaskManager.Project.Application.DTOs;
using TaskManager.Project.Domain.Interfaces;
using TaskManager.Shared.Application.Exceptions;

namespace TaskManager.Project.Application.Queries.GetProjectMembers;

public class GetProjectMembersQueryHandler : IRequestHandler<GetProjectMembersQuery, IReadOnlyList<ProjectMemberResponse>>
{
    private readonly IProjectRepository _projectRepository;

    public GetProjectMembersQueryHandler(IProjectRepository projectRepository)
    {
        _projectRepository = projectRepository;
    }

    public async Task<IReadOnlyList<ProjectMemberResponse>> Handle(GetProjectMembersQuery request, CancellationToken ct)
    {
        var project = await _projectRepository.GetByIdAsync(request.ProjectId, ct)
            ?? throw new NotFoundException("Project", request.ProjectId);

        return project.Members
            .Select(m => new ProjectMemberResponse(m.UserId, m.Role, m.JoinedAt))
            .ToList();
    }
}
