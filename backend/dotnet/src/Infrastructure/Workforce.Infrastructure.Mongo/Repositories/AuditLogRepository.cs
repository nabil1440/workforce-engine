using MongoDB.Driver;
using AppCoreSortDirection = Workforce.AppCore.Abstractions.Queries.SortDirection;
using Workforce.AppCore.Abstractions.Queries;
using Workforce.AppCore.Abstractions.Repositories;
using Workforce.AppCore.Domain.Audit;

namespace Workforce.Infrastructure.Mongo.Repositories;

public sealed class AuditLogRepository : IAuditLogRepository
{
    private readonly IMongoCollection<AuditLog> _collection;

    public AuditLogRepository(MongoContext context)
    {
        _collection = context.AuditLogs;
    }

    public Task<PagedResult<AuditLog>> ListAsync(AuditQuery query, CancellationToken cancellationToken = default)
    {
        return ListInternalAsync(query, filter => filter, cancellationToken);
    }

    public Task<PagedResult<AuditLog>> ListByEntityAsync(string entityType, string entityId, AuditQuery query, CancellationToken cancellationToken = default)
    {
        return ListInternalAsync(query, filter =>
            filter & Builders<AuditLog>.Filter.Eq(a => a.EntityType, entityType)
                   & Builders<AuditLog>.Filter.Eq(a => a.EntityId, entityId),
            cancellationToken);
    }

    private async Task<PagedResult<AuditLog>> ListInternalAsync(
        AuditQuery query,
        Func<FilterDefinition<AuditLog>, FilterDefinition<AuditLog>> applyEntityFilter,
        CancellationToken cancellationToken)
    {
        var normalized = NormalizePaging(query.Page, query.PageSize);
        var filter = BuildFilter(query);
        filter = applyEntityFilter(filter);

        var totalCount = await _collection.CountDocumentsAsync(filter, cancellationToken: cancellationToken);
        var items = await _collection.Find(filter)
            .Sort(BuildSort(query.SortBy, query.SortDirection))
            .Skip((normalized.page - 1) * normalized.pageSize)
            .Limit(normalized.pageSize)
            .ToListAsync(cancellationToken);

        return new PagedResult<AuditLog>
        {
            Items = items,
            TotalCount = (int)totalCount,
            Page = normalized.page,
            PageSize = normalized.pageSize
        };
    }

    private static FilterDefinition<AuditLog> BuildFilter(AuditQuery query)
    {
        var filter = Builders<AuditLog>.Filter.Empty;

        if (!string.IsNullOrWhiteSpace(query.EventType))
        {
            filter &= Builders<AuditLog>.Filter.Eq(a => a.EventType, query.EventType);
        }

        if (!string.IsNullOrWhiteSpace(query.EntityType))
        {
            filter &= Builders<AuditLog>.Filter.Eq(a => a.EntityType, query.EntityType);
        }

        if (!string.IsNullOrWhiteSpace(query.EntityId))
        {
            filter &= Builders<AuditLog>.Filter.Eq(a => a.EntityId, query.EntityId);
        }

        if (!string.IsNullOrWhiteSpace(query.Actor))
        {
            filter &= Builders<AuditLog>.Filter.Eq(a => a.Actor, query.Actor);
        }

        return filter;
    }

    private static SortDefinition<AuditLog> BuildSort(string? sortBy, AppCoreSortDirection direction)
    {
        var key = sortBy?.Trim().ToLowerInvariant();
        var ascending = direction == AppCoreSortDirection.Asc;

        return key switch
        {
            "timestamp" => ascending ? Builders<AuditLog>.Sort.Ascending(a => a.Timestamp) : Builders<AuditLog>.Sort.Descending(a => a.Timestamp),
            "eventtype" => ascending ? Builders<AuditLog>.Sort.Ascending(a => a.EventType) : Builders<AuditLog>.Sort.Descending(a => a.EventType),
            _ => Builders<AuditLog>.Sort.Descending(a => a.Timestamp)
        };
    }

    private static (int page, int pageSize) NormalizePaging(int page, int pageSize)
    {
        var safePage = page <= 0 ? 1 : page;
        var safePageSize = pageSize <= 0 ? 20 : pageSize;
        return (safePage, safePageSize);
    }
}
