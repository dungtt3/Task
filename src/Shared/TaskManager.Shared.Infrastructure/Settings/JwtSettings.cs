namespace TaskManager.Shared.Infrastructure.Settings;

public class JwtSettings
{
    public string Secret { get; set; } = default!;
    public string Issuer { get; set; } = "TaskManager";
    public string Audience { get; set; } = "TaskManagerApp";
    public int ExpirationMinutes { get; set; } = 60;
    public int RefreshTokenExpirationDays { get; set; } = 7;
}
