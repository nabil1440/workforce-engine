using Microsoft.AspNetCore.Mvc;
using Workforce.Api.Contracts.Audit;
using Workforce.Api.Contracts.Common;
using Workforce.Api.Contracts.Queries;
using Workforce.AppCore.Abstractions.Queries;
using Workforce.AppCore.Domain.Audit;
using Workforce.AppCore.Services;

namespace Workforce.Api.Controllers;

[ApiController]
[Route("api/v1/audit")]
public sealed class AuditController : ControllerBase
{
    private readonly IAuditService _audit;

    public AuditController(IAuditService audit)
    {
        _audit = audit;
    }

    [HttpGet]
    public async Task<IActionResult> ListAsync(
        [FromQuery] AuditListQuery request,
        CancellationToken cancellationToken = default)
    {
        var query = new AuditQuery
        {
            Page = request.Page,
            PageSize = request.PageSize,
            EventType = request.EventType,
            EntityType = request.EntityType,
            EntityId = request.EntityId,
            Actor = request.Actor,
            SortBy = request.Sort,
            SortDirection = request.SortDirection
        };

        var result = await _audit.ListAsync(query, cancellationToken);
        return result.ToActionResult(this, value => MapPaged(value, MapAudit));
    }

    [HttpGet("entity/{entityType}/{entityId}")]
    public async Task<IActionResult> ListByEntityAsync(
        string entityType,
        string entityId,
        [FromQuery] AuditEntityQuery request,
        CancellationToken cancellationToken = default)
    {
        var query = new AuditQuery
        {
            Page = request.Page,
            PageSize = request.PageSize,
            SortBy = request.Sort,
            SortDirection = request.SortDirection
        };

        var result = await _audit.ListByEntityAsync(entityType, entityId, query, cancellationToken);
        return result.ToActionResult(this, value => MapPaged(value, MapAudit));
    }

    private static AuditLogResponse MapAudit(AuditLog audit)
    {
        return new AuditLogResponse
        {
            Id = audit.Id,
            EventType = audit.EventType,
            EntityType = audit.EntityType,
            EntityId = audit.EntityId,
            Timestamp = audit.Timestamp,
            Actor = audit.Actor,
            Before = audit.Before,
            After = audit.After
        };
    }

    private static PagedResponse<AuditLogResponse> MapPaged(PagedResult<AuditLog> pagedResult, Func<AuditLog, AuditLogResponse> map)
    {
        return new PagedResponse<AuditLogResponse>
        {
            Items = pagedResult.Items.Select(map).ToList(),
            TotalCount = pagedResult.TotalCount,
            Page = pagedResult.Page,
            PageSize = pagedResult.PageSize
        };
    }
}
