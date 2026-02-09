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

            var auditLog = new AuditLog
            {
                Id = Guid.NewGuid().ToString("N"),
                EventType = envelope.EventType,
                EntityType = entityType,
                EntityId = entityId,
                Timestamp = envelope.OccurredAt == default ? DateTimeOffset.UtcNow : envelope.OccurredAt,
                Actor = "system",
                Before = null,
                After = envelope.Payload
            };

            await _writer.AddAsync(auditLog);
            _channel.BasicAck(args.DeliveryTag, false);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to process audit event message.");
            _channel.BasicNack(args.DeliveryTag, false, true);
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

    public override Task StopAsync(CancellationToken cancellationToken)
    {
        _channel?.Close();
        _channel?.Dispose();
        return base.StopAsync(cancellationToken);
    }
}
