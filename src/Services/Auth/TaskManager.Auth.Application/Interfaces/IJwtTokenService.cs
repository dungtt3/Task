using TaskManager.Auth.Domain.Entities;

namespace TaskManager.Auth.Application.Interfaces;

public interface IJwtTokenService
{
    string GenerateAccessToken(User user);
    RefreshToken GenerateRefreshToken();
}
