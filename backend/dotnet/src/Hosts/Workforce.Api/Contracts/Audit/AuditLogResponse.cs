namespace Workforce.Api.Contracts.Audit;

public sealed class AuditLogResponse
{
    public string Id { get; init; } = string.Empty;
    public string EventType { get; init; } = string.Empty;
    public string EntityType { get; init; } = string.Empty;
    public string EntityId { get; init; } = string.Empty;
    public DateTimeOffset Timestamp { get; init; }
    public string Actor { get; init; } = string.Empty;
    public object? Before { get; init; }
    public object? After { get; init; }
}
