using MediatR;
using Microsoft.Extensions.Logging;
using TaskManager.Auth.Application.DTOs;
using TaskManager.Auth.Application.Interfaces;
using TaskManager.Auth.Domain.Interfaces;

namespace TaskManager.Auth.Application.Commands.Login;

public class LoginCommandHandler : IRequestHandler<LoginCommand, AuthResponse>
{
    private readonly IUserRepository _userRepository;
    private readonly IJwtTokenService _jwtTokenService;
    private readonly ILogger<LoginCommandHandler> _logger;

    public LoginCommandHandler(
        IUserRepository userRepository,
        IJwtTokenService jwtTokenService,
        ILogger<LoginCommandHandler> logger)
    {
        _userRepository = userRepository;
        _jwtTokenService = jwtTokenService;
        _logger = logger;
    }

    public async Task<AuthResponse> Handle(LoginCommand request, CancellationToken ct)
    {
        var user = await _userRepository.GetByEmailAsync(request.Email.ToLowerInvariant(), ct);

        if (user is null || !BCrypt.Net.BCrypt.Verify(request.Password, user.PasswordHash))
            throw new UnauthorizedAccessException("Invalid email or password.");

        var refreshToken = _jwtTokenService.GenerateRefreshToken();
        user.RefreshTokens.Add(refreshToken);
        user.UpdatedAt = DateTime.UtcNow;

        await _userRepository.UpdateAsync(user, ct);

        var accessToken = _jwtTokenService.GenerateAccessToken(user);

        _logger.LogInformation("User {Email} logged in successfully", user.Email);

        return new AuthResponse(
            user.Id,
            user.Email,
            user.DisplayName,
            accessToken,
            refreshToken.Token);
    }
}
