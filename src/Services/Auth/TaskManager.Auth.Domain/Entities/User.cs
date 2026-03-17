using TaskManager.Shared.Domain.Entities;

namespace TaskManager.Auth.Domain.Entities;

public class User : AuditableEntity
{
    public string Email { get; set; } = default!;
    public string PasswordHash { get; set; } = default!;
    public string DisplayName { get; set; } = default!;
    public string? Avatar { get; set; }
    public List<RefreshToken> RefreshTokens { get; set; } = [];
}
