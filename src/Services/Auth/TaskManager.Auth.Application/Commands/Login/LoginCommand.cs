using MediatR;
using TaskManager.Auth.Application.DTOs;

namespace TaskManager.Auth.Application.Commands.Login;

public record LoginCommand(string Email, string Password) : IRequest<AuthResponse>;
