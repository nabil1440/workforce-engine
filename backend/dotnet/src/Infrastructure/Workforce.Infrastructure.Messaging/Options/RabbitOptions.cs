namespace Workforce.Infrastructure.Messaging.Options;

public sealed class RabbitOptions
{
    public string HostName { get; set; } = "localhost";
    public int Port { get; set; } = 5672;
    public string UserName { get; set; } = "guest";
    public string Password { get; set; } = "guest";
    public string VirtualHost { get; set; } = "/";
    public string ExchangePrefix { get; set; } = "workforce.events";
    public string AuditQueueName { get; set; } = "workforce.audit";
    public string? ClientProvidedName { get; set; }
}
