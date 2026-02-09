using System.Globalization;
using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using Workforce.AppCore.Domain.Audit;
using Workforce.Infrastructure.Messaging;
using Workforce.Infrastructure.Messaging.Options;
using Workforce.Infrastructure.Mongo.Repositories;

namespace Workforce.AuditWorker;

public sealed class Worker : BackgroundService
{
    private static readonly string[] EventTypes =
    [
        "EmployeeCreated",
        "EmployeeUpdated",
        "EmployeeDeactivated",
        "ProjectCreated",
        "ProjectUpdated",
        "ProjectStatusChanged",
        "TaskCreated",
        "TaskAssigned",
        "TaskStatusChanged",
        "LeaveRequested",
        "LeaveApproved",
        "LeaveRejected",
        "LeaveCancelled"
    ];

    private readonly ILogger<Worker> _logger;
    private readonly IConnection _connection;
    private readonly RabbitOptions _options;
    private readonly IAuditLogWriter _writer;
    private IModel? _channel;

    private const string RetryHeader = "x-retry-count";

    public Worker(ILogger<Worker> logger, IConnection connection, RabbitOptions options, IAuditLogWriter writer)
    {
        _logger = logger;
        _connection = connection;
        _options = options;
        _writer = writer;
    }

    protected override Task ExecuteAsync(CancellationToken stoppingToken)
    {
        _channel = _connection.CreateModel();
        _channel.BasicQos(0, 10, false);

        _channel.QueueDeclare(
            queue: _options.AuditQueueName,
            durable: true,
            exclusive: false,
            autoDelete: false,
            arguments: null);

        _channel.QueueDeclare(
            queue: _options.AuditRetryQueueName,
            durable: true,
            exclusive: false,
            autoDelete: false,
            arguments: new Dictionary<string, object>
            {
                ["x-dead-letter-exchange"] = string.Empty,
                ["x-dead-letter-routing-key"] = _options.AuditQueueName
            });

        _channel.QueueDeclare(
            queue: _options.AuditDeadLetterQueueName,
            durable: true,
            exclusive: false,
            autoDelete: false,
            arguments: null);

        foreach (var eventType in EventTypes)
        {
            var exchangeName = BuildExchangeName(eventType);
            _channel.ExchangeDeclare(exchange: exchangeName, type: ExchangeType.Direct, durable: true, autoDelete: false);
            _channel.QueueBind(_options.AuditQueueName, exchangeName, eventType);
        }

        var consumer = new AsyncEventingBasicConsumer(_channel);
        consumer.Received += HandleMessageAsync;

        _channel.BasicConsume(
            queue: _options.AuditQueueName,
            autoAck: false,
            consumer: consumer);

        _logger.LogInformation("Audit worker listening on queue {queue}.", _options.AuditQueueName);
        return Task.CompletedTask;
    }

    private async Task HandleMessageAsync(object sender, BasicDeliverEventArgs args)
    {
        if (_channel is null)
        {
            return;
        }

        try
        {
            var envelope = JsonSerializer.Deserialize<EventEnvelope>(args.Body.Span);
            if (envelope is null)
            {
                _channel.BasicAck(args.DeliveryTag, false);
                return;
            }

            var (entityType, entityId) = ResolveEntityInfo(envelope.Payload);
            var actor = ResolveActor(envelope.Payload);
            var before = ResolveSnapshot(envelope.Payload, "Before");
            var after = ResolveSnapshot(envelope.Payload, "After") ?? envelope.Payload;

            var auditId = ComputeAuditId(args.Body);
            var auditLog = new AuditLog
            {
                Id = auditId,
                EventType = envelope.EventType,
                EntityType = entityType,
                EntityId = entityId,
                Timestamp = envelope.OccurredAt == default ? DateTimeOffset.UtcNow : envelope.OccurredAt,
                Actor = actor,
                Before = before,
                After = after
            };

            await _writer.UpsertAsync(auditLog);
            _channel.BasicAck(args.DeliveryTag, false);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to process audit event message.");
            HandleRetry(args);
        }
    }

    private string BuildExchangeName(string eventType)
    {
        return string.IsNullOrWhiteSpace(_options.ExchangePrefix)
            ? eventType
            : $"{_options.ExchangePrefix}.{eventType}";
    }

    private static (string entityType, string entityId) ResolveEntityInfo(JsonElement payload)
    {
        if (payload.ValueKind != JsonValueKind.Object)
        {
            return ("Unknown", string.Empty);
        }

        var mappings = new (string Property, string EntityType)[]
        {
            ("EmployeeId", "Employee"),
            ("ProjectId", "Project"),
            ("TaskId", "Task"),
            ("LeaveId", "LeaveRequest")
        };

        foreach (var (property, entityType) in mappings)
        {
            if (!payload.TryGetProperty(property, out var value))
            {
                continue;
            }

            var entityId = value.ValueKind switch
            {
                JsonValueKind.Number when value.TryGetInt32(out var intValue) => intValue.ToString(),
                JsonValueKind.String => value.GetString() ?? string.Empty,
                _ => value.ToString()
            };

            return (entityType, entityId);
        }

        return ("Unknown", string.Empty);
    }

    private static string ResolveActor(JsonElement payload)
    {
        if (payload.ValueKind == JsonValueKind.Object && payload.TryGetProperty("Actor", out var actor))
        {
            return actor.GetString() ?? "system";
        }

        return "system";
    }

    private static JsonElement? ResolveSnapshot(JsonElement payload, string propertyName)
    {
        if (payload.ValueKind == JsonValueKind.Object && payload.TryGetProperty(propertyName, out var value))
        {
            return value;
        }

        return null;
    }

    private void HandleRetry(BasicDeliverEventArgs args)
    {
        if (_channel is null)
        {
            return;
        }

        var currentRetry = GetRetryCount(args.BasicProperties);
        if (currentRetry >= _options.MaxRetries)
        {
            PublishToQueue(_options.AuditDeadLetterQueueName, args, currentRetry);
            _channel.BasicAck(args.DeliveryTag, false);
            return;
        }

        var nextRetry = currentRetry + 1;
        var delaySeconds = _options.RetryBaseDelaySeconds * (int)Math.Pow(2, currentRetry);
        PublishToQueue(_options.AuditRetryQueueName, args, nextRetry, delaySeconds * 1000);
        _channel.BasicAck(args.DeliveryTag, false);
    }

    private int GetRetryCount(IBasicProperties? properties)
    {
        if (properties?.Headers is null)
        {
            return 0;
        }

        if (!properties.Headers.TryGetValue(RetryHeader, out var value) || value is null)
        {
            return 0;
        }

        return value switch
        {
            byte[] bytes => int.TryParse(Encoding.UTF8.GetString(bytes), out var parsed) ? parsed : 0,
            int retry => retry,
            long retry => (int)retry,
            _ => 0
        };
    }

    private void PublishToQueue(string queueName, BasicDeliverEventArgs args, int retryCount, int? delayMilliseconds = null)
    {
        if (_channel is null)
        {
            return;
        }

        var properties = _channel.CreateBasicProperties();
        properties.Persistent = true;
        properties.ContentType = args.BasicProperties?.ContentType ?? "application/json";
        properties.Headers = new Dictionary<string, object>
        {
            [RetryHeader] = retryCount.ToString(CultureInfo.InvariantCulture)
        };

        if (delayMilliseconds.HasValue)
        {
            properties.Expiration = delayMilliseconds.Value.ToString(CultureInfo.InvariantCulture);
        }

        _channel.BasicPublish(
            exchange: string.Empty,
            routingKey: queueName,
            basicProperties: properties,
            body: args.Body);
    }

    private static string ComputeAuditId(ReadOnlyMemory<byte> messageBody)
    {
        var bytes = SHA256.HashData(messageBody.Span);
        return Convert.ToHexString(bytes).ToLowerInvariant();
    }

    public override Task StopAsync(CancellationToken cancellationToken)
    {
        _channel?.Close();
        _channel?.Dispose();
        return base.StopAsync(cancellationToken);
    }
}
