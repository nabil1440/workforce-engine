using Workforce.AppCore.Abstractions.Repositories;
using Workforce.AppCore.Abstractions.Results;
using Workforce.AppCore.Domain.Dashboard;

namespace Workforce.AppCore.Services.Implementations;

public sealed class DashboardService : IDashboardService
{
    private readonly IDashboardSummaryRepository _dashboard;

    public DashboardService(IDashboardSummaryRepository dashboard)
    {
        _dashboard = dashboard;
    }

    public async Task<Result<DashboardSummary>> GetSummaryAsync(CancellationToken cancellationToken = default)
    {
        var summary = await _dashboard.GetLatestAsync(cancellationToken);
        if (summary is not null)
        {
            return Result<DashboardSummary>.Success(summary);
        }

        summary = new DashboardSummary
        {
            Id = "latest",
            GeneratedAt = DateTimeOffset.UtcNow,
            ActiveProjectsCount = 0,
            HeadcountByDepartment = [],
            TasksByStatus = [],
            LeaveStats = []
        };

        return Result<DashboardSummary>.Success(summary);
    }
}
