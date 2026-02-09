using Microsoft.AspNetCore.Mvc;
using Workforce.Api.Contracts.Common;
using Workforce.Api.Contracts.Projects;
using Workforce.Api.Contracts.Queries;
using Workforce.AppCore.Abstractions.Queries;
using Workforce.AppCore.Domain.Projects;
using Workforce.AppCore.Services;

namespace Workforce.Api.Controllers;

[ApiController]
[Route("api/v1/projects")]
public sealed class ProjectsController : ControllerBase
{
    private readonly IProjectService _projects;

    public ProjectsController(IProjectService projects)
    {
        _projects = projects;
    }

    [HttpGet]
    public async Task<IActionResult> ListAsync(
        [FromQuery] ProjectListQuery request,
        CancellationToken cancellationToken = default)
    {
        var query = new ProjectQuery
        {
            Page = request.Page,
            PageSize = request.PageSize,
            Status = request.Status,
            Search = request.Search,
            SortBy = request.Sort,
            SortDirection = request.SortDirection
        };

        var result = await _projects.ListAsync(query, cancellationToken);
        return result.ToActionResult(this, value => MapPaged(value, MapProject));
    }

    [HttpGet("{id:int}")]
    public async Task<IActionResult> GetByIdAsync(int id, CancellationToken cancellationToken)
    {
        var result = await _projects.GetByIdAsync(id, cancellationToken);
        return result.ToActionResult(this, MapProject);
    }

    [HttpPost]
    public async Task<IActionResult> CreateAsync([FromBody] ProjectRequest request, CancellationToken cancellationToken)
    {
        var project = MapProject(request);
        var result = await _projects.CreateAsync(project, cancellationToken);

        if (!result.IsSuccess || result.Value is null)
        {
            return result.ToActionResult(this);
        }

        return CreatedAtAction(nameof(GetByIdAsync), new { id = result.Value.Id }, MapProject(result.Value));
    }

    [HttpPut("{id:int}")]
    public async Task<IActionResult> UpdateAsync(int id, [FromBody] ProjectRequest request, CancellationToken cancellationToken)
    {
        var project = MapProject(request);
        project.Id = id;
        var result = await _projects.UpdateAsync(project, cancellationToken);
        return result.ToActionResult(this, MapProject);
    }

    private static Project MapProject(ProjectRequest request)
    {
        return new Project
        {
            Name = request.Name,
            Description = request.Description,
            Status = request.Status,
            StartDate = request.StartDate,
            EndDate = request.EndDate
        };
    }

    private static ProjectResponse MapProject(Project project)
    {
        return new ProjectResponse
        {
            Id = project.Id,
            Name = project.Name,
            Description = project.Description,
            Status = project.Status,
            StartDate = project.StartDate,
            EndDate = project.EndDate
        };
    }

    private static PagedResponse<ProjectResponse> MapPaged(PagedResult<Project> pagedResult, Func<Project, ProjectResponse> map)
    {
        return new PagedResponse<ProjectResponse>
        {
            Items = pagedResult.Items.Select(map).ToList(),
            TotalCount = pagedResult.TotalCount,
            Page = pagedResult.Page,
            PageSize = pagedResult.PageSize
        };
    }
}
