using Workforce.AppCore.Abstractions;
using Workforce.AppCore.Abstractions.Queries;
using Workforce.AppCore.Abstractions.Repositories;
using Workforce.AppCore.Abstractions.Results;
using Workforce.AppCore.Domain.Events;
using Workforce.AppCore.Domain.Leaves;
using Workforce.AppCore.Validation;

namespace Workforce.AppCore.Services.Implementations;

public sealed class LeaveService : ILeaveService
{
    private readonly ILeaveRequestRepository _leaves;
    private readonly IEventPublisher _events;

    public LeaveService(ILeaveRequestRepository leaves, IEventPublisher events)
    {
        _leaves = leaves;
        _events = events;
    }

    public async Task<Result<PagedResult<LeaveRequest>>> ListAsync(LeaveQuery query, CancellationToken cancellationToken = default)
    {
        var validation = Guard.NotNull(query, nameof(query));
        if (!validation.IsSuccess)
        {
            return Result<PagedResult<LeaveRequest>>.Fail(validation.Error);
        }

        var result = await _leaves.ListAsync(query, cancellationToken);
        return Result<PagedResult<LeaveRequest>>.Success(result);
    }

    public async Task<Result<LeaveRequest>> GetByIdAsync(string id, CancellationToken cancellationToken = default)
    {
        var validation = Guard.NotEmpty(id, nameof(id));
        if (!validation.IsSuccess)
        {
            return Result<LeaveRequest>.Fail(validation.Error);
        }

        var leave = await _leaves.GetByIdAsync(id, cancellationToken);
        return leave is null
            ? Result<LeaveRequest>.Fail(Errors.NotFound("LeaveRequest", id))
            : Result<LeaveRequest>.Success(leave);
    }

    public async Task<Result<LeaveRequest>> CreateAsync(LeaveRequest leaveRequest, CancellationToken cancellationToken = default)
    {
        var validation = LeaveRules.Validate(leaveRequest);
        if (!validation.IsSuccess)
        {
            return Result<LeaveRequest>.Fail(validation.Error);
        }

        leaveRequest.Status = LeaveStatus.Pending;
        if (leaveRequest.CreatedAt == default)
        {
            leaveRequest.CreatedAt = DateTimeOffset.UtcNow;
        }

        var id = await _leaves.AddAsync(leaveRequest, cancellationToken);
        leaveRequest.Id = id;
        await _events.PublishAsync(new LeaveRequested(leaveRequest.Id, "system", null, leaveRequest), cancellationToken);
        return Result<LeaveRequest>.Success(leaveRequest);
    }

    public Task<Result<LeaveRequest>> ApproveAsync(string id, string changedBy, string? comment, CancellationToken cancellationToken = default)
    {
        return UpdateStatusAsync(id, changedBy, comment, LeaveStatus.Approved, cancellationToken);
    }

    public Task<Result<LeaveRequest>> RejectAsync(string id, string changedBy, string? comment, CancellationToken cancellationToken = default)
    {
        return UpdateStatusAsync(id, changedBy, comment, LeaveStatus.Rejected, cancellationToken);
    }

    public Task<Result<LeaveRequest>> CancelAsync(string id, string changedBy, string? comment, CancellationToken cancellationToken = default)
    {
        return UpdateStatusAsync(id, changedBy, comment, LeaveStatus.Cancelled, cancellationToken);
    }

    private async Task<Result<LeaveRequest>> UpdateStatusAsync(
        string id,
        string changedBy,
        string? comment,
        LeaveStatus toStatus,
        CancellationToken cancellationToken)
    {
        var validation = Guard.NotEmpty(id, nameof(id));
        if (!validation.IsSuccess)
        {
            return Result<LeaveRequest>.Fail(validation.Error);
        }

        validation = Guard.NotEmpty(changedBy, nameof(changedBy));
        if (!validation.IsSuccess)
        {
            return Result<LeaveRequest>.Fail(validation.Error);
        }

        var leave = await _leaves.GetByIdAsync(id, cancellationToken);
        if (leave is null)
        {
            return Result<LeaveRequest>.Fail(Errors.NotFound("LeaveRequest", id));
        }

        var before = new LeaveRequest
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
            ApprovalHistory = leave.ApprovalHistory.Select(entry => new LeaveRequest.ApprovalHistoryEntry
            {
                Status = entry.Status,
                ChangedBy = entry.ChangedBy,
                ChangedAt = entry.ChangedAt,
                Comment = entry.Comment
            }).ToList()
        };
        validation = LeaveRules.ValidateApproval(leave.Status, toStatus);
        if (!validation.IsSuccess)
        {
            return Result<LeaveRequest>.Fail(validation.Error);
        }

        leave.Status = toStatus;
        leave.ApprovalHistory ??= [];
        leave.ApprovalHistory.Add(new LeaveRequest.ApprovalHistoryEntry
        {
            Status = toStatus,
            ChangedBy = changedBy,
            ChangedAt = DateTimeOffset.UtcNow,
            Comment = comment
        });

        await _leaves.UpdateAsync(leave, cancellationToken);
        await PublishLeaveEvent(id, toStatus, before, leave, cancellationToken);
        return Result<LeaveRequest>.Success(leave);
    }

    private Task PublishLeaveEvent(string id, LeaveStatus status, LeaveRequest before, LeaveRequest after, CancellationToken cancellationToken)
    {
        return status switch
        {
            LeaveStatus.Approved => _events.PublishAsync(new LeaveApproved(id, "system", before, after), cancellationToken),
            LeaveStatus.Rejected => _events.PublishAsync(new LeaveRejected(id, "system", before, after), cancellationToken),
            LeaveStatus.Cancelled => _events.PublishAsync(new LeaveCancelled(id, "system", before, after), cancellationToken),
            _ => Task.CompletedTask
        };
    }
}
