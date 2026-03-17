namespace TaskManager.Shared.Infrastructure.Settings;

public class RedisSettings
{
    public string ConnectionString { get; set; } = "localhost:6379";
    public int DefaultExpirationSeconds { get; set; } = 300;
}
