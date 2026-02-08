using Workforce.AppCore.Abstractions.Queries;
using Workforce.AppCore.Domain.Leaves;

namespace Workforce.AppCore.Services;

public interface ILeaveService
{
    Task<PagedResult<LeaveRequest>> ListAsync(LeaveQuery query, CancellationToken cancellationToken = default);
    Task<LeaveRequest?> GetByIdAsync(string id, CancellationToken cancellationToken = default);
    Task<LeaveRequest> CreateAsync(LeaveRequest leaveRequest, CancellationToken cancellationToken = default);
    Task<LeaveRequest> ApproveAsync(string id, string changedBy, string? comment, CancellationToken cancellationToken = default);
    Task<LeaveRequest> RejectAsync(string id, string changedBy, string? comment, CancellationToken cancellationToken = default);
    Task<LeaveRequest> CancelAsync(string id, string changedBy, string? comment, CancellationToken cancellationToken = default);
}
