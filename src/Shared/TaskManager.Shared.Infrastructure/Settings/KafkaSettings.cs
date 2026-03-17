namespace TaskManager.Shared.Infrastructure.Settings;

public class KafkaSettings
{
    public string BootstrapServers { get; set; } = "localhost:9092";
    public string GroupId { get; set; } = default!;
}
