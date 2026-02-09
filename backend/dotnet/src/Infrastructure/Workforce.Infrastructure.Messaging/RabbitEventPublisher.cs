using System.Text;
using System.Text.Json;
using RabbitMQ.Client;
using Workforce.AppCore.Abstractions;
using Workforce.Infrastructure.Messaging.Options;

namespace Workforce.Infrastructure.Messaging;

public sealed class RabbitEventPublisher : IEventPublisher
{
    private readonly IConnection _connection;
    private readonly RabbitOptions _options;

    public RabbitEventPublisher(IConnection connection, RabbitOptions options)
    {
        _connection = connection;
        _options = options;
    }

    public Task PublishAsync(IDomainEvent domainEvent, CancellationToken cancellationToken = default)
    {
        var eventType = domainEvent.GetType().Name;
        var exchangeName = BuildExchangeName(eventType);
        var payload = JsonSerializer.SerializeToElement(domainEvent, domainEvent.GetType());
        var envelope = new EventEnvelope(eventType, domainEvent.OccurredAt, payload);
        var body = JsonSerializer.SerializeToUtf8Bytes(envelope);

        using var channel = _connection.CreateModel();
        channel.ExchangeDeclare(exchange: exchangeName, type: ExchangeType.Direct, durable: true, autoDelete: false);

        var properties = channel.CreateBasicProperties();
        properties.Persistent = true;
        properties.ContentType = "application/json";

        channel.BasicPublish(
            exchange: exchangeName,
            routingKey: eventType,
            basicProperties: properties,
            body: body);

        return Task.CompletedTask;
    }

    private string BuildExchangeName(string eventType)
    {
        return string.IsNullOrWhiteSpace(_options.ExchangePrefix)
            ? eventType
            : $"{_options.ExchangePrefix}.{eventType}";
    }
}
