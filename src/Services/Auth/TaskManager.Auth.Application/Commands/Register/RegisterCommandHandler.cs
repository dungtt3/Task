using MediatR;
using Microsoft.Extensions.Logging;
using TaskManager.Auth.Application.DTOs;
using TaskManager.Auth.Application.Interfaces;
using TaskManager.Auth.Domain.Entities;
using TaskManager.Auth.Domain.Interfaces;

namespace TaskManager.Auth.Application.Commands.Register;

public class RegisterCommandHandler : IRequestHandler<RegisterCommand, AuthResponse>
{
    private readonly IUserRepository _userRepository;
    private readonly IJwtTokenService _jwtTokenService;
    private readonly ILogger<RegisterCommandHandler> _logger;

    public RegisterCommandHandler(
        IUserRepository userRepository,
        IJwtTokenService jwtTokenService,
        ILogger<RegisterCommandHandler> logger)
    {
        _userRepository = userRepository;
        _jwtTokenService = jwtTokenService;
        _logger = logger;
    }

    public async Task<AuthResponse> Handle(RegisterCommand request, CancellationToken ct)
    {
        var existingUser = await _userRepository.GetByEmailAsync(request.Email, ct);
        if (existingUser is not null)
            throw new InvalidOperationException("A user with this email already exists.");

        var user = new User
        {
            Email = request.Email.ToLowerInvariant(),
            PasswordHash = BCrypt.Net.BCrypt.HashPassword(request.Password),
            DisplayName = request.DisplayName
        };

        var refreshToken = _jwtTokenService.GenerateRefreshToken();
        user.RefreshTokens.Add(refreshToken);

        await _userRepository.CreateAsync(user, ct);

        var accessToken = _jwtTokenService.GenerateAccessToken(user);

        _logger.LogInformation("User {Email} registered successfully", user.Email);

        return new AuthResponse(
            user.Id,
            user.Email,
            user.DisplayName,
            accessToken,
            refreshToken.Token);
    }
}
