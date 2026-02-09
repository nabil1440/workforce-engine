using Workforce.AppCore.Abstractions.Queries;

namespace Workforce.Api.Contracts.Queries;

public sealed class AuditListQuery
{
    public int Page { get; init; } = 1;
    public int PageSize { get; init; } = 20;
    public string? EventType { get; init; }
    public string? EntityType { get; init; }
    public string? EntityId { get; init; }
    public string? Actor { get; init; }
    public string? Sort { get; init; }
    public SortDirection SortDirection { get; init; } = SortDirection.Desc;
}
