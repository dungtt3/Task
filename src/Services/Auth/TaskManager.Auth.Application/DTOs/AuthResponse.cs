namespace TaskManager.Auth.Application.DTOs;

public record AuthResponse(
    string Id,
    string Email,
    string DisplayName,
    string AccessToken,
    string RefreshToken);

public record UserDto(
    string Id,
    string Email,
    string DisplayName,
    string? Avatar);
