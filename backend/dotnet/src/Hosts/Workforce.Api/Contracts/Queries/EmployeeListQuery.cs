using Workforce.AppCore.Abstractions.Queries;

namespace Workforce.Api.Contracts.Queries;

public sealed class EmployeeListQuery
{
    public int Page { get; init; } = 1;
    public int PageSize { get; init; } = 20;
    public int? DepartmentId { get; init; }
    public bool? IsActive { get; init; }
    public string? Search { get; init; }
    public string? Sort { get; init; }
    public SortDirection SortDirection { get; init; } = SortDirection.Asc;
}
