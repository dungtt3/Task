using MediatR;
using Microsoft.Extensions.Logging;
using TaskManager.Auth.Domain.Interfaces;

namespace TaskManager.Auth.Application.Commands.Logout;

public class LogoutCommandHandler : IRequestHandler<LogoutCommand, bool>
{
    private readonly IUserRepository _userRepository;
    private readonly ILogger<LogoutCommandHandler> _logger;

    public LogoutCommandHandler(IUserRepository userRepository, ILogger<LogoutCommandHandler> logger)
    {
        _userRepository = userRepository;
        _logger = logger;
    }

    public async Task<bool> Handle(LogoutCommand request, CancellationToken ct)
    {
        var user = await _userRepository.GetByIdAsync(request.UserId, ct);
        if (user is null) return false;

        var token = user.RefreshTokens.FirstOrDefault(t => t.Token == request.RefreshToken);
        if (token is not null)
        {
            token.Revoked = DateTime.UtcNow;
            user.UpdatedAt = DateTime.UtcNow;
            await _userRepository.UpdateAsync(user, ct);
        }

        _logger.LogInformation("User {UserId} logged out", request.UserId);

        return true;
    }
}
