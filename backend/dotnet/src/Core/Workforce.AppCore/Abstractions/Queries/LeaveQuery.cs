using Workforce.AppCore.Domain.Leaves;

namespace Workforce.AppCore.Abstractions.Queries;

public sealed class LeaveQuery : PagedQuery
{
    public LeaveStatus? Status { get; init; }
    public LeaveType? LeaveType { get; init; }
}
