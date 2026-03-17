using MediatR;
using Microsoft.Extensions.Logging;
using TaskManager.Auth.Application.DTOs;
using TaskManager.Auth.Application.Interfaces;
using TaskManager.Auth.Domain.Interfaces;

namespace TaskManager.Auth.Application.Commands.RefreshToken;

public class RefreshTokenCommandHandler : IRequestHandler<RefreshTokenCommand, AuthResponse>
{
    private readonly IUserRepository _userRepository;
    private readonly IJwtTokenService _jwtTokenService;
    private readonly ILogger<RefreshTokenCommandHandler> _logger;

    public RefreshTokenCommandHandler(
        IUserRepository userRepository,
        IJwtTokenService jwtTokenService,
        ILogger<RefreshTokenCommandHandler> logger)
    {
        _userRepository = userRepository;
        _jwtTokenService = jwtTokenService;
        _logger = logger;
    }

    public async Task<AuthResponse> Handle(RefreshTokenCommand request, CancellationToken ct)
    {
        var user = await _userRepository.GetByRefreshTokenAsync(request.Token, ct);

        if (user is null)
            throw new UnauthorizedAccessException("Invalid refresh token.");

        var existingToken = user.RefreshTokens.FirstOrDefault(t => t.Token == request.Token);

        if (existingToken is null || !existingToken.IsActive)
            throw new UnauthorizedAccessException("Refresh token is expired or revoked.");

        // Revoke old token
        existingToken.Revoked = DateTime.UtcNow;

        // Generate new tokens
        var newRefreshToken = _jwtTokenService.GenerateRefreshToken();
        user.RefreshTokens.Add(newRefreshToken);
        user.UpdatedAt = DateTime.UtcNow;

        await _userRepository.UpdateAsync(user, ct);

        var accessToken = _jwtTokenService.GenerateAccessToken(user);

        _logger.LogInformation("Token refreshed for user {Email}", user.Email);

        return new AuthResponse(
            user.Id,
            user.Email,
            user.DisplayName,
            accessToken,
            newRefreshToken.Token);
    }
}
