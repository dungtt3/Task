using MediatR;
using TaskManager.Auth.Application.DTOs;
using TaskManager.Auth.Domain.Interfaces;
using TaskManager.Shared.Application.Exceptions;

namespace TaskManager.Auth.Application.Queries.GetMe;

public class GetMeQueryHandler : IRequestHandler<GetMeQuery, UserDto>
{
    private readonly IUserRepository _userRepository;

    public GetMeQueryHandler(IUserRepository userRepository)
    {
        _userRepository = userRepository;
    }

    public async Task<UserDto> Handle(GetMeQuery request, CancellationToken ct)
    {
        var user = await _userRepository.GetByIdAsync(request.UserId, ct)
            ?? throw new NotFoundException("User", request.UserId);

        return new UserDto(user.Id, user.Email, user.DisplayName, user.Avatar);
    }
}
