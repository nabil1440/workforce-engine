namespace Workforce.AppCore.Abstractions.Queries;

public sealed class AuditQuery : PagedQuery
{
    public string? EventType { get; init; }
    public string? EntityType { get; init; }
    public string? EntityId { get; init; }
    public string? Actor { get; init; }
}
