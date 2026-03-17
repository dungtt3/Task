using MediatR;

namespace TaskManager.Auth.Application.Commands.Logout;

public record LogoutCommand(string UserId, string RefreshToken) : IRequest<bool>;
