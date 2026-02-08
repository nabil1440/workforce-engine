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
        return summary is null
            ? Result<DashboardSummary>.Fail(Errors.NotFound("DashboardSummary", "latest"))
            : Result<DashboardSummary>.Success(summary);
    }
}
