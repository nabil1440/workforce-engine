using Microsoft.AspNetCore.Mvc;
using Workforce.Api.Contracts.Common;
using Workforce.Api.Contracts.Queries;
using Workforce.Api.Contracts.Tasks;
using Workforce.AppCore.Abstractions.Queries;
using ProjectTaskStatus = Workforce.AppCore.Domain.Projects.TaskStatus;
using Workforce.AppCore.Domain.Projects;
using Workforce.AppCore.Services;

namespace Workforce.Api.Controllers;

[ApiController]
public sealed class TasksController : ControllerBase
{
    private readonly ITaskService _tasks;

    public TasksController(ITaskService tasks)
    {
        _tasks = tasks;
    }

    [HttpPost("api/v1/projects/{projectId:int}/tasks")]
    public async Task<IActionResult> CreateAsync(int projectId, [FromBody] TaskRequest request, CancellationToken cancellationToken)
    {
        var task = MapTask(request, projectId);
        var result = await _tasks.CreateAsync(task, cancellationToken);

        if (!result.IsSuccess || result.Value is null)
        {
            return result.ToActionResult(this);
        }

        return CreatedAtAction(nameof(GetByIdAsync), new { taskId = result.Value.Id }, MapTask(result.Value));
    }

    [HttpGet("api/v1/projects/{projectId:int}/tasks")]
    public async Task<IActionResult> ListByProjectAsync(
        int projectId,
        [FromQuery] TaskListQuery request,
        CancellationToken cancellationToken = default)
    {
        var query = new TaskQuery
        {
            Page = request.Page,
            PageSize = request.PageSize,
            Status = request.Status,
            AssignedEmployeeId = request.AssignedEmployeeId,
            SortBy = request.Sort,
            SortDirection = request.SortDirection
        };

        var result = await _tasks.ListByProjectAsync(projectId, query, cancellationToken);
        return result.ToActionResult(this, value => MapPaged(value, MapTask));
    }

    [HttpGet("api/v1/tasks/{taskId:int}")]
    public async Task<IActionResult> GetByIdAsync(int taskId, CancellationToken cancellationToken)
    {
        var result = await _tasks.GetByIdAsync(taskId, cancellationToken);
        return result.ToActionResult(this, MapTask);
    }

    [HttpPut("api/v1/tasks/{taskId:int}")]
    public async Task<IActionResult> UpdateAsync(int taskId, [FromBody] TaskRequest request, CancellationToken cancellationToken)
    {
        var existingResult = await _tasks.GetByIdAsync(taskId, cancellationToken);
        if (!existingResult.IsSuccess || existingResult.Value is null)
        {
            return existingResult.ToActionResult(this, MapTask);
        }

        var task = MapTask(request, existingResult.Value.ProjectId);
        task.Id = taskId;
        var result = await _tasks.UpdateAsync(task, cancellationToken);
        return result.ToActionResult(this, MapTask);
    }

    [HttpPost("api/v1/tasks/{taskId:int}/transition")]
    public async Task<IActionResult> TransitionAsync(int taskId, [FromBody] TaskTransitionRequest request, CancellationToken cancellationToken)
    {
        var result = await _tasks.TransitionAsync(taskId, request.ToStatus, cancellationToken);
        return result.ToActionResult(this, MapTask);
    }

    private static WorkTask MapTask(TaskRequest request, int projectId)
    {
        return new WorkTask
        {
            ProjectId = projectId > 0 ? projectId : request.ProjectId ?? 0,
            AssignedEmployeeId = request.AssignedEmployeeId,
            Title = request.Title,
            Description = request.Description,
            Status = request.Status,
            Priority = request.Priority,
            DueDate = request.DueDate
        };
    }

    private static TaskResponse MapTask(WorkTask task)
    {
        return new TaskResponse
        {
            Id = task.Id,
            ProjectId = task.ProjectId,
            AssignedEmployeeId = task.AssignedEmployeeId,
            Title = task.Title,
            Description = task.Description,
            Status = task.Status,
            Priority = task.Priority,
            DueDate = task.DueDate
        };
    }

    private static PagedResponse<TaskResponse> MapPaged(PagedResult<WorkTask> pagedResult, Func<WorkTask, TaskResponse> map)
    {
        return new PagedResponse<TaskResponse>
        {
            Items = pagedResult.Items.Select(map).ToList(),
            TotalCount = pagedResult.TotalCount,
            Page = pagedResult.Page,
            PageSize = pagedResult.PageSize
        };
    }
}
