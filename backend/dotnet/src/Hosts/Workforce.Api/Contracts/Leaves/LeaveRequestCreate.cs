using Workforce.AppCore.Domain.Leaves;

namespace Workforce.Api.Contracts.Leaves;

public sealed class LeaveRequestCreate
{
    public int EmployeeId { get; init; }
    public string EmployeeName { get; init; } = string.Empty;
    public LeaveType LeaveType { get; init; }
    public DateOnly StartDate { get; init; }
    public DateOnly EndDate { get; init; }
    public string? Reason { get; init; }
}
