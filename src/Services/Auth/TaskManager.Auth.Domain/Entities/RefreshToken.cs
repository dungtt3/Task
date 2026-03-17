namespace TaskManager.Auth.Domain.Entities;

public class RefreshToken
{
    public string Token { get; set; } = default!;
    public DateTime Expires { get; set; }
    public DateTime Created { get; set; } = DateTime.UtcNow;
    public DateTime? Revoked { get; set; }
    public bool IsExpired => DateTime.UtcNow >= Expires;
    public bool IsActive => Revoked is null && !IsExpired;
}
