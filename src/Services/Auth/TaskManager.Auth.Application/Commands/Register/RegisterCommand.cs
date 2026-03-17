using MediatR;
using TaskManager.Auth.Application.DTOs;

namespace TaskManager.Auth.Application.Commands.Register;

public record RegisterCommand(
    string Email,
    string Password,
    string DisplayName) : IRequest<AuthResponse>;
