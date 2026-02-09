using Microsoft.AspNetCore.Mvc;
using Workforce.Api.Contracts.Common;
using Workforce.Api.Contracts.Leaves;
using Workforce.Api.Contracts.Queries;
using Workforce.AppCore.Abstractions.Queries;
using Workforce.AppCore.Domain.Leaves;
using Workforce.AppCore.Services;

namespace Workforce.Api.Controllers;

[ApiController]
[Route("api/v1/leaves")]
public sealed class LeavesController : ControllerBase
{
    private readonly ILeaveService _leaves;

    public LeavesController(ILeaveService leaves)
    {
        _leaves = leaves;
    }

    [HttpGet]
    public async Task<IActionResult> ListAsync(
        [FromQuery] LeaveListQuery request,
        CancellationToken cancellationToken = default)
    {
        var query = new LeaveQuery
        {
            Page = request.Page,
            PageSize = request.PageSize,
            Status = request.Status,
            LeaveType = request.LeaveType,
            SortBy = request.Sort,
            SortDirection = request.SortDirection
        };

        var result = await _leaves.ListAsync(query, cancellationToken);
        return result.ToActionResult(this, value => MapPaged(value, MapLeave));
    }

    [HttpGet("{id}")]
    public async Task<IActionResult> GetByIdAsync(string id, CancellationToken cancellationToken)
    {
        var result = await _leaves.GetByIdAsync(id, cancellationToken);
        return result.ToActionResult(this, MapLeave);
    }

    [HttpPost]
    public async Task<IActionResult> CreateAsync([FromBody] LeaveRequestCreate request, CancellationToken cancellationToken)
    {
        var leaveRequest = new LeaveRequest
        {
            EmployeeId = request.EmployeeId,
            EmployeeName = request.EmployeeName,
            LeaveType = request.LeaveType,
            StartDate = request.StartDate,
            EndDate = request.EndDate,
            Reason = request.Reason
        };

        var result = await _leaves.CreateAsync(leaveRequest, cancellationToken);

        if (!result.IsSuccess || result.Value is null)
        {
            return result.ToActionResult(this);
        }

        return CreatedAtAction(nameof(GetByIdAsync), new { id = result.Value.Id }, MapLeave(result.Value));
    }

    [HttpPost("{id}/approve")]
    public async Task<IActionResult> ApproveAsync(string id, [FromBody] LeaveDecisionRequest request, CancellationToken cancellationToken)
    {
        var result = await _leaves.ApproveAsync(id, request.ChangedBy, request.Comment, cancellationToken);
        return result.ToActionResult(this, MapLeave);
    }

    [HttpPost("{id}/reject")]
    public async Task<IActionResult> RejectAsync(string id, [FromBody] LeaveDecisionRequest request, CancellationToken cancellationToken)
    {
        var result = await _leaves.RejectAsync(id, request.ChangedBy, request.Comment, cancellationToken);
        return result.ToActionResult(this, MapLeave);
    }

    [HttpPost("{id}/cancel")]
    public async Task<IActionResult> CancelAsync(string id, [FromBody] LeaveDecisionRequest request, CancellationToken cancellationToken)
    {
        var result = await _leaves.CancelAsync(id, request.ChangedBy, request.Comment, cancellationToken);
        return result.ToActionResult(this, MapLeave);
    }

    private static LeaveRequestResponse MapLeave(LeaveRequest leave)
    {
        return new LeaveRequestResponse
        {
            Id = leave.Id,
            EmployeeId = leave.EmployeeId,
            EmployeeName = leave.EmployeeName,
            LeaveType = leave.LeaveType,
            StartDate = leave.StartDate,
            EndDate = leave.EndDate,
            Status = leave.Status,
            Reason = leave.Reason,
            CreatedAt = leave.CreatedAt,
            ApprovalHistory = leave.ApprovalHistory.Select(entry => new LeaveRequestResponse.ApprovalHistoryResponse
            {
                Status = entry.Status,
                ChangedBy = entry.ChangedBy,
                ChangedAt = entry.ChangedAt,
                Comment = entry.Comment
            }).ToList()
        };
    }

    private static PagedResponse<LeaveRequestResponse> MapPaged(PagedResult<LeaveRequest> pagedResult, Func<LeaveRequest, LeaveRequestResponse> map)
    {
        return new PagedResponse<LeaveRequestResponse>
        {
            Items = pagedResult.Items.Select(map).ToList(),
            TotalCount = pagedResult.TotalCount,
            Page = pagedResult.Page,
            PageSize = pagedResult.PageSize
        };
    }
}
