using Workforce.AppCore.Abstractions.Queries;

namespace Workforce.Api.Contracts.Queries;

public sealed class AuditEntityQuery
{
    public int Page { get; init; } = 1;
    public int PageSize { get; init; } = 20;
    public string? Sort { get; init; }
    public SortDirection SortDirection { get; init; } = SortDirection.Desc;
}
