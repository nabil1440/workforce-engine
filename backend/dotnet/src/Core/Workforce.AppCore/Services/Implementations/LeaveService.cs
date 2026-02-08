using Workforce.AppCore.Abstractions.Queries;
using Workforce.AppCore.Abstractions.Repositories;
using Workforce.AppCore.Abstractions.Results;
using Workforce.AppCore.Domain.Leaves;
using Workforce.AppCore.Validation;

namespace Workforce.AppCore.Services.Implementations;

public sealed class LeaveService : ILeaveService
{
    private readonly ILeaveRequestRepository _leaves;

    public LeaveService(ILeaveRequestRepository leaves)
    {
        _leaves = leaves;
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
        return Result<LeaveRequest>.Success(leave);
    }
}
