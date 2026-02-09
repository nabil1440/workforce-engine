using Workforce.AppCore.Domain.Audit;

namespace Workforce.Infrastructure.Mongo.Repositories;

public interface IAuditLogWriter
{
    Task UpsertAsync(AuditLog auditLog, CancellationToken cancellationToken = default);
}
