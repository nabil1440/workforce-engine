using Workforce.AppCore.Domain.Audit;

namespace Workforce.Infrastructure.Mongo.Repositories;

public interface IAuditLogWriter
{
    Task AddAsync(AuditLog auditLog, CancellationToken cancellationToken = default);
}
