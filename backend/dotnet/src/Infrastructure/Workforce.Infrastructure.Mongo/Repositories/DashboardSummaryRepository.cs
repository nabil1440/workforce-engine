using MongoDB.Driver;
using Workforce.AppCore.Abstractions.Repositories;
using Workforce.AppCore.Domain.Dashboard;

namespace Workforce.Infrastructure.Mongo.Repositories;

public sealed class DashboardSummaryRepository : IDashboardSummaryRepository
{
    private readonly IMongoCollection<DashboardSummary> _collection;

    public DashboardSummaryRepository(MongoContext context)
    {
        _collection = context.DashboardSummaries;
    }

    public async Task<DashboardSummary?> GetLatestAsync(CancellationToken cancellationToken = default)
    {
        var summary = await _collection.Find(_ => true)
            .SortByDescending(d => d.GeneratedAt)
            .FirstOrDefaultAsync(cancellationToken);

        return summary;
    }
}
