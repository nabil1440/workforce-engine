using Workforce.AppCore.Abstractions.Queries;
using Workforce.AppCore.Abstractions.Results;
using Workforce.AppCore.Domain.Audit;

namespace Workforce.AppCore.Services;

public interface IAuditService
{
    Task<Result<PagedResult<AuditLog>>> ListAsync(AuditQuery query, CancellationToken cancellationToken = default);
    Task<Result<PagedResult<AuditLog>>> ListByEntityAsync(string entityType, string entityId, AuditQuery query, CancellationToken cancellationToken = default);
}
