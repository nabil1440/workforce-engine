using MongoDB.Driver;
using Workforce.AppCore.Domain.Audit;

namespace Workforce.Infrastructure.Mongo.Repositories;

public sealed class AuditLogWriter : IAuditLogWriter
{
    private readonly IMongoCollection<AuditLog> _collection;

    public AuditLogWriter(MongoContext context)
    {
        _collection = context.AuditLogs;
    }

    public Task UpsertAsync(AuditLog auditLog, CancellationToken cancellationToken = default)
    {
        return _collection.ReplaceOneAsync(
            log => log.Id == auditLog.Id,
            auditLog,
            new ReplaceOptions { IsUpsert = true },
            cancellationToken);
    }
}
