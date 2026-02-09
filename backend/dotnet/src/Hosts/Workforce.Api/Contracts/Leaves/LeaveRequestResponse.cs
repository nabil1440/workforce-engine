using Workforce.AppCore.Domain.Leaves;

namespace Workforce.Api.Contracts.Leaves;

public sealed class LeaveRequestResponse
{
    public string Id { get; init; } = string.Empty;
    public int EmployeeId { get; init; }
    public string EmployeeName { get; init; } = string.Empty;
    public LeaveType LeaveType { get; init; }
    public DateOnly StartDate { get; init; }
    public DateOnly EndDate { get; init; }
    public LeaveStatus Status { get; init; }
    public string? Reason { get; init; }
    public DateTimeOffset CreatedAt { get; init; }
    public IReadOnlyList<ApprovalHistoryResponse> ApprovalHistory { get; init; } = Array.Empty<ApprovalHistoryResponse>();

    public sealed class ApprovalHistoryResponse
    {
        public LeaveStatus Status { get; init; }
        public string ChangedBy { get; init; } = string.Empty;
        public DateTimeOffset ChangedAt { get; init; }
        public string? Comment { get; init; }
    }
}
