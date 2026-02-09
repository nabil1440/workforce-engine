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

    public Task AddAsync(AuditLog auditLog, CancellationToken cancellationToken = default)
    {
        return _collection.InsertOneAsync(auditLog, cancellationToken: cancellationToken);
    }
}
