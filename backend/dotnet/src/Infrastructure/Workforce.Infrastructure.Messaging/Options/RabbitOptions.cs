namespace Workforce.Infrastructure.Messaging.Options;

public sealed class RabbitOptions
{
    public string HostName { get; set; } = "localhost";
    public int Port { get; set; } = 5672;
    public string UserName { get; set; } = null!;
    public string Password { get; set; } = null!;
    public string VirtualHost { get; set; } = "/";
    public string ExchangePrefix { get; set; } = "workforce.events";
    public string AuditQueueName { get; set; } = "workforce.audit";
    public string AuditRetryQueueName { get; set; } = "workforce.audit.retry";
    public string AuditDeadLetterQueueName { get; set; } = "workforce.audit.dlq";
    public int MaxRetries { get; set; } = 3;
    public int RetryBaseDelaySeconds { get; set; } = 5;
    public string? ClientProvidedName { get; set; }
}
