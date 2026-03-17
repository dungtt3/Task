using System.Text.Json;
using Confluent.Kafka;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using TaskManager.Shared.Application.Interfaces;
using TaskManager.Shared.Infrastructure.Settings;

namespace TaskManager.Shared.Infrastructure.Messaging;

public class KafkaProducer : IKafkaProducer, IDisposable
{
    private readonly IProducer<string, string> _producer;
    private readonly ILogger<KafkaProducer> _logger;

    public KafkaProducer(IOptions<KafkaSettings> settings, ILogger<KafkaProducer> logger)
    {
        _logger = logger;
        var config = new ProducerConfig
        {
            BootstrapServers = settings.Value.BootstrapServers,
            EnableIdempotence = true,
            Acks = Acks.All
        };
        _producer = new ProducerBuilder<string, string>(config).Build();
    }

    public async Task PublishAsync<T>(string topic, string key, T message, CancellationToken ct = default)
    {
        var json = JsonSerializer.Serialize(message);

        _logger.LogInformation("Publishing to {Topic} with key {Key}", topic, key);

        await _producer.ProduceAsync(topic, new Message<string, string>
        {
            Key = key,
            Value = json
        }, ct);
    }

    public void Dispose()
    {
        _producer.Flush(TimeSpan.FromSeconds(5));
        _producer.Dispose();
    }
}
