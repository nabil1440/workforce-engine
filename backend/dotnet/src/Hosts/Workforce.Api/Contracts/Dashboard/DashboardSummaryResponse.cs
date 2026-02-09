namespace Workforce.Api.Contracts.Dashboard;

public sealed class DashboardSummaryResponse
{
    public string Id { get; init; } = string.Empty;
    public DateTimeOffset GeneratedAt { get; init; }
    public IReadOnlyList<DepartmentCountResponse> HeadcountByDepartment { get; init; } = Array.Empty<DepartmentCountResponse>();
    public int ActiveProjectsCount { get; init; }
    public IReadOnlyList<StatusCountResponse> TasksByStatus { get; init; } = Array.Empty<StatusCountResponse>();
    public IReadOnlyList<LeaveStatResponse> LeaveStats { get; init; } = Array.Empty<LeaveStatResponse>();

    public sealed class DepartmentCountResponse
    {
        public string Department { get; init; } = string.Empty;
        public int Count { get; init; }
    }

    public sealed class StatusCountResponse
    {
        public string Status { get; init; } = string.Empty;
        public int Count { get; init; }
    }

    public sealed class LeaveStatResponse
    {
        public string Type { get; init; } = string.Empty;
        public string Status { get; init; } = string.Empty;
        public int Count { get; init; }
    }
}
