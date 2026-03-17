using System.Text.Json;
using Confluent.Kafka;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using TaskManager.Shared.Infrastructure.Settings;

namespace TaskManager.Shared.Infrastructure.Messaging;

public abstract class KafkaConsumerBase<TMessage> : BackgroundService
{
    private readonly IConsumer<string, string> _consumer;
    private readonly ILogger _logger;
    private readonly IServiceScopeFactory _scopeFactory;
    private readonly string _topic;

    protected KafkaConsumerBase(
        IOptions<KafkaSettings> settings,
        ILogger logger,
        IServiceScopeFactory scopeFactory,
        string topic)
    {
        _logger = logger;
        _scopeFactory = scopeFactory;
        _topic = topic;

        var config = new ConsumerConfig
        {
            BootstrapServers = settings.Value.BootstrapServers,
            GroupId = settings.Value.GroupId,
            AutoOffsetReset = AutoOffsetReset.Earliest,
            EnableAutoCommit = false
        };
        _consumer = new ConsumerBuilder<string, string>(config).Build();
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        _consumer.Subscribe(_topic);

        _logger.LogInformation("Kafka consumer started for topic {Topic}", _topic);

        await Task.Run(() =>
        {
            while (!stoppingToken.IsCancellationRequested)
            {
                try
                {
                    var result = _consumer.Consume(stoppingToken);

                    if (result?.Message?.Value is null)
                        continue;

                    var message = JsonSerializer.Deserialize<TMessage>(result.Message.Value);

                    if (message is not null)
                    {
                        using var scope = _scopeFactory.CreateScope();
                        HandleAsync(message, scope.ServiceProvider, stoppingToken)
                            .GetAwaiter().GetResult();
                    }

                    _consumer.Commit(result);
                }
                catch (OperationCanceledException)
                {
                    break;
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Error consuming message from {Topic}", _topic);
                }
            }
        }, stoppingToken);

        _consumer.Close();
    }

    protected abstract Task HandleAsync(TMessage message, IServiceProvider services, CancellationToken ct);
}
