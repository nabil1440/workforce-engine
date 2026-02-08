using Workforce.AppCore.Abstractions.Queries;
using Workforce.AppCore.Abstractions.Repositories;
using Workforce.AppCore.Abstractions.Results;
using Workforce.AppCore.Domain.Leaves;
using Workforce.AppCore.Services.Implementations;

namespace Workforce.AppCore.Tests;

public class LeaveServiceTests
{
    [Fact]
    public async Task CreateAsync_SetsPendingStatusAndCreatedAt()
    {
        var repo = new FakeLeaveRepository();
        var service = new LeaveService(repo);

        var leave = new LeaveRequest
        {
            EmployeeId = 1,
            EmployeeName = "Jane Doe",
            LeaveType = LeaveType.Sick,
            StartDate = new DateOnly(2024, 2, 1),
            EndDate = new DateOnly(2024, 2, 2)
        };

        var result = await service.CreateAsync(leave);

        Assert.True(result.IsSuccess);
        Assert.Equal(LeaveStatus.Pending, result.Value!.Status);
        Assert.NotEqual(default, result.Value.CreatedAt);
    }

    [Fact]
    public async Task ApproveAsync_FailsWhenAlreadyApproved()
    {
        var repo = new FakeLeaveRepository();
        var service = new LeaveService(repo);

        var leave = new LeaveRequest
        {
            Id = "leave-1",
            EmployeeId = 1,
            EmployeeName = "Jane Doe",
            LeaveType = LeaveType.Annual,
            StartDate = new DateOnly(2024, 3, 1),
            EndDate = new DateOnly(2024, 3, 2),
            Status = LeaveStatus.Approved
        };

        repo.Seed(leave);

        var result = await service.ApproveAsync(leave.Id, "manager", null);

        Assert.False(result.IsSuccess);
        Assert.Equal(Errors.InvalidState("leave status is already set to the requested value"), result.Error);
    }

    private sealed class FakeLeaveRepository : ILeaveRequestRepository
    {
        private readonly Dictionary<string, LeaveRequest> _store = new();

        public void Seed(LeaveRequest leaveRequest)
        {
            _store[leaveRequest.Id] = leaveRequest;
        }

        public Task<PagedResult<LeaveRequest>> ListAsync(LeaveQuery query, CancellationToken cancellationToken = default)
        {
            var items = _store.Values.ToList();
            return Task.FromResult(new PagedResult<LeaveRequest>
            {
                Items = items,
                TotalCount = items.Count,
                Page = query.Page,
                PageSize = query.PageSize
            });
        }

        public Task<LeaveRequest?> GetByIdAsync(string id, CancellationToken cancellationToken = default)
        {
            _store.TryGetValue(id, out var leave);
            return Task.FromResult(leave);
        }

        public Task<string> AddAsync(LeaveRequest leaveRequest, CancellationToken cancellationToken = default)
        {
            var id = string.IsNullOrWhiteSpace(leaveRequest.Id) ? Guid.NewGuid().ToString("N") : leaveRequest.Id;
            _store[id] = leaveRequest;
            return Task.FromResult(id);
        }

        public Task UpdateAsync(LeaveRequest leaveRequest, CancellationToken cancellationToken = default)
        {
            _store[leaveRequest.Id] = leaveRequest;
            return Task.CompletedTask;
        }
    }
}
