using Workforce.AppCore.Domain.Dashboard;

namespace Workforce.AppCore.Services;

public interface IDashboardService
{
    Task<DashboardSummary?> GetSummaryAsync(CancellationToken cancellationToken = default);
}
