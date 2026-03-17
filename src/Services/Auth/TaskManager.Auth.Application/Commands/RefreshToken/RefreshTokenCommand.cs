using MediatR;
using TaskManager.Auth.Application.DTOs;

namespace TaskManager.Auth.Application.Commands.RefreshToken;

public record RefreshTokenCommand(string Token) : IRequest<AuthResponse>;
