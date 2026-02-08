using Workforce.AppCore.Domain.Dashboard;

namespace Workforce.AppCore.Abstractions.Repositories;

public interface IDashboardSummaryRepository
{
    Task<DashboardSummary?> GetLatestAsync(CancellationToken cancellationToken = default);
}
