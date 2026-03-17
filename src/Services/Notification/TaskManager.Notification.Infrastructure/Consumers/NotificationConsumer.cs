using System.Text.Json;
using Confluent.Kafka;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using TaskManager.Notification.Domain.Interfaces;
using TaskManager.Shared.Domain.Enums;
using TaskManager.Shared.Infrastructure.Settings;

namespace TaskManager.Notification.Infrastructure.Consumers;

public class NotificationConsumer : BackgroundService
{
    private readonly IConsumer<string, string> _consumer;
    private readonly IServiceScopeFactory _scopeFactory;
    private readonly ILogger<NotificationConsumer> _logger;
    private readonly string[] _topics = ["task.assigned", "task.completed", "project.updated"];

    public NotificationConsumer(
        IOptions<KafkaSettings> settings,
        IServiceScopeFactory scopeFactory,
        ILogger<NotificationConsumer> logger)
    {
        _scopeFactory = scopeFactory;
        _logger = logger;

        var config = new ConsumerConfig
        {
            BootstrapServers = settings.Value.BootstrapServers,
            GroupId = "notification-group",
            AutoOffsetReset = AutoOffsetReset.Earliest,
            EnableAutoCommit = false
        };
        _consumer = new ConsumerBuilder<string, string>(config).Build();
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        _consumer.Subscribe(_topics);
        _logger.LogInformation("Notification consumer started for topics: {Topics}", string.Join(", ", _topics));

        await Task.Run(() =>
        {
            while (!stoppingToken.IsCancellationRequested)
            {
                try
                {
                    var result = _consumer.Consume(stoppingToken);
                    if (result?.Message?.Value is null) continue;

                    ProcessMessageAsync(result.Topic, result.Message.Value)
                        .GetAwaiter().GetResult();

                    _consumer.Commit(result);
                }
                catch (OperationCanceledException) { break; }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Error consuming notification message");
                }
            }
        }, stoppingToken);

        _consumer.Close();
    }

    private async Task ProcessMessageAsync(string topic, string message)
    {
        using var scope = _scopeFactory.CreateScope();
        var repository = scope.ServiceProvider.GetRequiredService<INotificationRepository>();

        var doc = JsonDocument.Parse(message);
        var root = doc.RootElement;

        var notification = topic switch
        {
            "task.assigned" => new Domain.Entities.Notification
            {
                UserId = root.GetProperty("AssigneeId").GetString() ?? "",
                Title = "Task Assigned",
                Message = $"You have been assigned to task: {root.GetProperty("Title").GetString()}",
                Type = NotificationType.TaskAssigned,
                ReferenceId = root.GetProperty("TaskId").GetString() ?? "",
                ReferenceType = ReferenceType.Task
            },
            "task.completed" => new Domain.Entities.Notification
            {
                UserId = root.GetProperty("AssigneeId").GetString() ?? "",
                Title = "Task Completed",
                Message = $"Task completed: {root.GetProperty("Title").GetString()}",
                Type = NotificationType.TaskCompleted,
                ReferenceId = root.GetProperty("TaskId").GetString() ?? "",
                ReferenceType = ReferenceType.Task
            },
            "project.updated" => new Domain.Entities.Notification
            {
                UserId = root.GetProperty("MemberUserId").GetString() ?? "",
                Title = "Project Updated",
                Message = $"Project '{root.GetProperty("Name").GetString()}' has been updated",
                Type = NotificationType.ProjectUpdate,
                ReferenceId = root.GetProperty("Id").GetString() ?? "",
                ReferenceType = ReferenceType.Project
            },
            _ => null
        };

        if (notification is not null)
        {
            await repository.CreateAsync(notification);
            _logger.LogInformation("Created notification for user {UserId}: {Title}", notification.UserId, notification.Title);
        }
    }
}
