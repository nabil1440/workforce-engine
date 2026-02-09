namespace Workforce.Api.Contracts.Leaves;

public sealed class LeaveDecisionRequest
{
    public string ChangedBy { get; init; } = string.Empty;
    public string? Comment { get; init; }
}
