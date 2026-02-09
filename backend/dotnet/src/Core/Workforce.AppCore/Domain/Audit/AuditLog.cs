namespace Workforce.AppCore.Domain.Audit;

public sealed class AuditLog
{
    public string Id { get; set; } = string.Empty;
    public string EventType { get; set; } = string.Empty;
    public string EntityType { get; set; } = string.Empty;
    public string EntityId { get; set; } = string.Empty;
    public DateTimeOffset Timestamp { get; set; }
    public string Actor { get; set; } = string.Empty;
    public object? Before { get; set; }
    public object? After { get; set; }
}
