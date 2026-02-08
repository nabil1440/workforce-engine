using Workforce.AppCore.Abstractions.Results;
using Workforce.AppCore.Domain.Dashboard;

namespace Workforce.AppCore.Services;

public interface IDashboardService
{
    Task<Result<DashboardSummary>> GetSummaryAsync(CancellationToken cancellationToken = default);
}
