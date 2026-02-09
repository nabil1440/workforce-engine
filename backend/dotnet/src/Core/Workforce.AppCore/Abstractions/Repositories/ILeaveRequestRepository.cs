using Workforce.AppCore.Abstractions.Queries;
using Workforce.AppCore.Domain.Leaves;

namespace Workforce.AppCore.Abstractions.Repositories;

public interface ILeaveRequestRepository
{
    Task<PagedResult<LeaveRequest>> ListAsync(LeaveQuery query, CancellationToken cancellationToken = default);
    Task<LeaveRequest?> GetByIdAsync(string id, CancellationToken cancellationToken = default);
    Task<string> AddAsync(LeaveRequest leaveRequest, CancellationToken cancellationToken = default);
    Task UpdateAsync(LeaveRequest leaveRequest, CancellationToken cancellationToken = default);
}
