using Workforce.AppCore.Abstractions.Repositories;
using Workforce.AppCore.Domain.Dashboard;
using Workforce.AppCore.Services.Implementations;

namespace Workforce.AppCore.Tests;

public sealed class DashboardServiceTests
{
    [Fact]
    public async Task GetSummaryAsync_ReturnsEmptySummaryWhenMissing()
    {
        var repo = new FakeDashboardSummaryRepository(null);
        var service = new DashboardService(repo);

        var result = await service.GetSummaryAsync();

        Assert.True(result.IsSuccess);
        Assert.NotNull(result.Value);
        Assert.Equal("latest", result.Value!.Id);
        Assert.Equal(0, result.Value.ActiveProjectsCount);
        Assert.Empty(result.Value.HeadcountByDepartment);
        Assert.Empty(result.Value.TasksByStatus);
        Assert.Empty(result.Value.LeaveStats);
    }

    [Fact]
    public async Task GetSummaryAsync_ReturnsLatestSummary()
    {
        var summary = new DashboardSummary
        {
            Id = "summary-1",
            GeneratedAt = DateTimeOffset.UtcNow,
            ActiveProjectsCount = 3,
            HeadcountByDepartment = [new DashboardSummary.DepartmentCount { Department = "Engineering", Count = 2 }]
        };

        var repo = new FakeDashboardSummaryRepository(summary);
        var service = new DashboardService(repo);

        var result = await service.GetSummaryAsync();

        Assert.True(result.IsSuccess);
        Assert.NotNull(result.Value);
        Assert.Equal("summary-1", result.Value!.Id);
        Assert.Equal(3, result.Value.ActiveProjectsCount);
        Assert.Single(result.Value.HeadcountByDepartment);
    }

    private sealed class FakeDashboardSummaryRepository : IDashboardSummaryRepository
    {
        private readonly DashboardSummary? _summary;

        public FakeDashboardSummaryRepository(DashboardSummary? summary)
        {
            _summary = summary;
        }

        public Task<DashboardSummary?> GetLatestAsync(CancellationToken cancellationToken = default)
        {
            return Task.FromResult(_summary);
        }
    }
}
