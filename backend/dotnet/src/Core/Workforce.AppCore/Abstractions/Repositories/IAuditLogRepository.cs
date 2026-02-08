using Workforce.AppCore.Abstractions.Queries;
using Workforce.AppCore.Domain.Audit;

namespace Workforce.AppCore.Abstractions.Repositories;

public interface IAuditLogRepository
{
    Task<PagedResult<AuditLog>> ListAsync(AuditQuery query, CancellationToken cancellationToken = default);
    Task<PagedResult<AuditLog>> ListByEntityAsync(string entityType, string entityId, AuditQuery query, CancellationToken cancellationToken = default);
}
