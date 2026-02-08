namespace Workforce.AppCore.Domain.Dashboard;

public sealed class DashboardSummary
{
    public string Id { get; set; } = string.Empty;
    public DateTimeOffset GeneratedAt { get; set; }
    public List<DepartmentCount> HeadcountByDepartment { get; set; } = [];
    public int ActiveProjectsCount { get; set; }
    public List<StatusCount> TasksByStatus { get; set; } = [];
    public List<LeaveStat> LeaveStats { get; set; } = [];

    public sealed class DepartmentCount
    {
        public string Department { get; set; } = string.Empty;
        public int Count { get; set; }
    }

    public sealed class StatusCount
    {
        public string Status { get; set; } = string.Empty;
        public int Count { get; set; }
    }

    public sealed class LeaveStat
    {
        public string Type { get; set; } = string.Empty;
        public string Status { get; set; } = string.Empty;
        public int Count { get; set; }
    }
}
