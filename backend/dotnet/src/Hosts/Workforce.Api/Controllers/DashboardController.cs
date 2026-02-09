using Microsoft.AspNetCore.Mvc;
using Workforce.Api.Contracts.Common;
using Workforce.Api.Contracts.Dashboard;
using Workforce.AppCore.Domain.Dashboard;
using Workforce.AppCore.Services;

namespace Workforce.Api.Controllers;

[ApiController]
[Route("api/v1/dashboard")]
public sealed class DashboardController : ControllerBase
{
    private readonly IDashboardService _dashboard;

    public DashboardController(IDashboardService dashboard)
    {
        _dashboard = dashboard;
    }

    [HttpGet("summary")]
    public async Task<IActionResult> GetSummaryAsync(CancellationToken cancellationToken)
    {
        var result = await _dashboard.GetSummaryAsync(cancellationToken);
        return result.ToActionResult(this, MapSummary);
    }

    private static DashboardSummaryResponse MapSummary(DashboardSummary summary)
    {
        return new DashboardSummaryResponse
        {
            Id = summary.Id,
            GeneratedAt = summary.GeneratedAt,
            ActiveProjectsCount = summary.ActiveProjectsCount,
            HeadcountByDepartment = summary.HeadcountByDepartment.Select(entry => new DashboardSummaryResponse.DepartmentCountResponse
            {
                Department = entry.Department,
                Count = entry.Count
            }).ToList(),
            TasksByStatus = summary.TasksByStatus.Select(entry => new DashboardSummaryResponse.StatusCountResponse
            {
                Status = entry.Status,
                Count = entry.Count
            }).ToList(),
            LeaveStats = summary.LeaveStats.Select(entry => new DashboardSummaryResponse.LeaveStatResponse
            {
                Type = entry.Type,
                Status = entry.Status,
                Count = entry.Count
            }).ToList()
        };
    }
}
