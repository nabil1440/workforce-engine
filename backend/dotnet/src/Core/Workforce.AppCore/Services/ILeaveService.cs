using Workforce.AppCore.Abstractions.Queries;
using Workforce.AppCore.Abstractions.Results;
using Workforce.AppCore.Domain.Leaves;

namespace Workforce.AppCore.Services;

public interface ILeaveService
{
    Task<Result<PagedResult<LeaveRequest>>> ListAsync(LeaveQuery query, CancellationToken cancellationToken = default);
    Task<Result<LeaveRequest>> GetByIdAsync(string id, CancellationToken cancellationToken = default);
    Task<Result<LeaveRequest>> CreateAsync(LeaveRequest leaveRequest, CancellationToken cancellationToken = default);
    Task<Result<LeaveRequest>> ApproveAsync(string id, string changedBy, string? comment, CancellationToken cancellationToken = default);
    Task<Result<LeaveRequest>> RejectAsync(string id, string changedBy, string? comment, CancellationToken cancellationToken = default);
    Task<Result<LeaveRequest>> CancelAsync(string id, string changedBy, string? comment, CancellationToken cancellationToken = default);
}
