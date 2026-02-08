using Workforce.AppCore.Abstractions.Queries;
using Workforce.AppCore.Abstractions.Repositories;
using Workforce.AppCore.Abstractions.Results;
using Workforce.AppCore.Domain.Audit;
using Workforce.AppCore.Validation;

namespace Workforce.AppCore.Services.Implementations;

public sealed class AuditService : IAuditService
{
    private readonly IAuditLogRepository _auditLogs;

    public AuditService(IAuditLogRepository auditLogs)
    {
        _auditLogs = auditLogs;
    }

    public async Task<Result<PagedResult<AuditLog>>> ListAsync(AuditQuery query, CancellationToken cancellationToken = default)
    {
        var validation = Guard.NotNull(query, nameof(query));
        if (!validation.IsSuccess)
        {
            return Result<PagedResult<AuditLog>>.Fail(validation.Error);
        }

        var result = await _auditLogs.ListAsync(query, cancellationToken);
        return Result<PagedResult<AuditLog>>.Success(result);
    }

    public async Task<Result<PagedResult<AuditLog>>> ListByEntityAsync(string entityType, string entityId, AuditQuery query, CancellationToken cancellationToken = default)
    {
        var validation = Guard.NotEmpty(entityType, nameof(entityType));
        if (!validation.IsSuccess)
        {
            return Result<PagedResult<AuditLog>>.Fail(validation.Error);
        }

        validation = Guard.NotEmpty(entityId, nameof(entityId));
        if (!validation.IsSuccess)
        {
            return Result<PagedResult<AuditLog>>.Fail(validation.Error);
        }

        validation = Guard.NotNull(query, nameof(query));
        if (!validation.IsSuccess)
        {
            return Result<PagedResult<AuditLog>>.Fail(validation.Error);
        }

        var result = await _auditLogs.ListByEntityAsync(entityType, entityId, query, cancellationToken);
        return Result<PagedResult<AuditLog>>.Success(result);
    }
}
