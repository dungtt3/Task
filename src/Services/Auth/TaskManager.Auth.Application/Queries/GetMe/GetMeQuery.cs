using MediatR;
using TaskManager.Auth.Application.DTOs;

namespace TaskManager.Auth.Application.Queries.GetMe;

public record GetMeQuery(string UserId) : IRequest<UserDto>;
