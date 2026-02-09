namespace Workforce.AppCore.Domain.Leaves;

public sealed class LeaveRequest
{
    public string Id { get; set; } = string.Empty;
    public int EmployeeId { get; set; }
    public string EmployeeName { get; set; } = string.Empty;
    public LeaveType LeaveType { get; set; }
    public DateOnly StartDate { get; set; }
    public DateOnly EndDate { get; set; }
    public LeaveStatus Status { get; set; }
    public string? Reason { get; set; }
    public List<ApprovalHistoryEntry> ApprovalHistory { get; set; } = [];
    public DateTimeOffset CreatedAt { get; set; }

    public sealed class ApprovalHistoryEntry
    {
        public LeaveStatus Status { get; set; }
        public string ChangedBy { get; set; } = string.Empty;
        public DateTimeOffset ChangedAt { get; set; }
        public string? Comment { get; set; }
    }
}
