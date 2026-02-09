using Workforce.AppCore.Abstractions.Queries;
using Workforce.AppCore.Domain.Leaves;

namespace Workforce.Api.Contracts.Queries;

public sealed class LeaveListQuery
{
    public int Page { get; init; } = 1;
    public int PageSize { get; init; } = 20;
    public LeaveStatus? Status { get; init; }
    public LeaveType? LeaveType { get; init; }
    public string? Sort { get; init; }
    public SortDirection SortDirection { get; init; } = SortDirection.Asc;
}
