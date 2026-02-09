using MongoDB.Driver;
using AppCoreSortDirection = Workforce.AppCore.Abstractions.Queries.SortDirection;
using Workforce.AppCore.Abstractions.Queries;
using Workforce.AppCore.Abstractions.Repositories;
using Workforce.AppCore.Domain.Leaves;

namespace Workforce.Infrastructure.Mongo.Repositories;

public sealed class LeaveRequestRepository : ILeaveRequestRepository
{
    private readonly IMongoCollection<LeaveRequest> _collection;

    public LeaveRequestRepository(MongoContext context)
    {
        _collection = context.LeaveRequests;
    }

    public async Task<PagedResult<LeaveRequest>> ListAsync(LeaveQuery query, CancellationToken cancellationToken = default)
    {
        var normalized = NormalizePaging(query.Page, query.PageSize);
        var filter = BuildFilter(query);
        var sort = BuildSort(query.SortBy, query.SortDirection);

        var totalCount = await _collection.CountDocumentsAsync(filter, cancellationToken: cancellationToken);
        var items = await _collection.Find(filter)
            .Sort(sort)
            .Skip((normalized.page - 1) * normalized.pageSize)
            .Limit(normalized.pageSize)
            .ToListAsync(cancellationToken);

        return new PagedResult<LeaveRequest>
        {
            Items = items,
            TotalCount = (int)totalCount,
            Page = normalized.page,
            PageSize = normalized.pageSize
        };
    }

    public async Task<LeaveRequest?> GetByIdAsync(string id, CancellationToken cancellationToken = default)
    {
        var cursor = await _collection.FindAsync(l => l.Id == id, cancellationToken: cancellationToken);
        return await cursor.FirstOrDefaultAsync(cancellationToken);
    }

    public async Task<string> AddAsync(LeaveRequest leaveRequest, CancellationToken cancellationToken = default)
    {
        await _collection.InsertOneAsync(leaveRequest, cancellationToken: cancellationToken);
        return leaveRequest.Id;
    }

    public async Task UpdateAsync(LeaveRequest leaveRequest, CancellationToken cancellationToken = default)
    {
        await _collection.ReplaceOneAsync(l => l.Id == leaveRequest.Id, leaveRequest, cancellationToken: cancellationToken);
    }

    private static FilterDefinition<LeaveRequest> BuildFilter(LeaveQuery query)
    {
        var filter = Builders<LeaveRequest>.Filter.Empty;

        if (query.Status.HasValue)
        {
            filter &= Builders<LeaveRequest>.Filter.Eq(l => l.Status, query.Status.Value);
        }

        if (query.LeaveType.HasValue)
        {
            filter &= Builders<LeaveRequest>.Filter.Eq(l => l.LeaveType, query.LeaveType.Value);
        }

        return filter;
    }

    private static SortDefinition<LeaveRequest> BuildSort(string? sortBy, AppCoreSortDirection direction)
    {
        var key = sortBy?.Trim().ToLowerInvariant();
        var ascending = direction == AppCoreSortDirection.Asc;

        return key switch
        {
            "startdate" => ascending ? Builders<LeaveRequest>.Sort.Ascending(l => l.StartDate) : Builders<LeaveRequest>.Sort.Descending(l => l.StartDate),
            "enddate" => ascending ? Builders<LeaveRequest>.Sort.Ascending(l => l.EndDate) : Builders<LeaveRequest>.Sort.Descending(l => l.EndDate),
            "createdat" => ascending ? Builders<LeaveRequest>.Sort.Ascending(l => l.CreatedAt) : Builders<LeaveRequest>.Sort.Descending(l => l.CreatedAt),
            "status" => ascending ? Builders<LeaveRequest>.Sort.Ascending(l => l.Status) : Builders<LeaveRequest>.Sort.Descending(l => l.Status),
            _ => Builders<LeaveRequest>.Sort.Descending(l => l.CreatedAt)
        };
    }

    private static (int page, int pageSize) NormalizePaging(int page, int pageSize)
    {
        var safePage = page <= 0 ? 1 : page;
        var safePageSize = pageSize <= 0 ? 20 : pageSize;
        return (safePage, safePageSize);
    }
}
