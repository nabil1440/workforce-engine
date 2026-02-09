using Microsoft.AspNetCore.Mvc;
using Workforce.Api.Contracts.Common;
using Workforce.Api.Contracts.Queries;
using Workforce.Api.Contracts.Tasks;
using Workforce.AppCore.Abstractions.Repositories;
using Workforce.AppCore.Abstractions.Queries;
using ProjectTaskStatus = Workforce.AppCore.Domain.Projects.TaskStatus;
using Workforce.AppCore.Domain.Projects;
using Workforce.AppCore.Services;

namespace Workforce.Api.Controllers;

[ApiController]
public sealed class TasksController : ControllerBase
{
    private readonly ITaskService _tasks;
    private readonly IEmployeeRepository _employees;
    private readonly IDepartmentRepository _departments;
    private readonly IDesignationRepository _designations;

    public TasksController(
        ITaskService tasks,
        IEmployeeRepository employees,
        IDepartmentRepository departments,
        IDesignationRepository designations)
    {
        _tasks = tasks;
        _employees = employees;
        _departments = departments;
        _designations = designations;
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

        var assignee = await BuildAssigneeAsync(result.Value, cancellationToken);
        return CreatedAtAction(nameof(GetByIdAsync), new { taskId = result.Value.Id }, MapTask(result.Value, assignee));
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
        if (!result.IsSuccess || result.Value is null)
        {
            return result.ToActionResult(this);
        }

        var assignees = await BuildAssigneesAsync(result.Value.Items, cancellationToken);
        return Ok(MapPaged(result.Value, task => MapTask(task, ResolveAssignee(assignees, task))));
    }

    [HttpGet("api/v1/tasks/{taskId:int}")]
    public async Task<IActionResult> GetByIdAsync(int taskId, CancellationToken cancellationToken)
    {
        var result = await _tasks.GetByIdAsync(taskId, cancellationToken);
        if (!result.IsSuccess || result.Value is null)
        {
            return result.ToActionResult(this);
        }

        var assignee = await BuildAssigneeAsync(result.Value, cancellationToken);
        return Ok(MapTask(result.Value, assignee));
    }

    [HttpPut("api/v1/tasks/{taskId:int}")]
    public async Task<IActionResult> UpdateAsync(int taskId, [FromBody] TaskRequest request, CancellationToken cancellationToken)
    {
        var existingResult = await _tasks.GetByIdAsync(taskId, cancellationToken);
        if (!existingResult.IsSuccess || existingResult.Value is null)
        {
            return existingResult.ToActionResult(this, task => MapTask(task));
        }

        var task = MapTask(request, existingResult.Value.ProjectId);
        task.Id = taskId;
        var result = await _tasks.UpdateAsync(task, cancellationToken);
        if (!result.IsSuccess || result.Value is null)
        {
            return result.ToActionResult(this, task => MapTask(task));
        }

        var assignee = await BuildAssigneeAsync(result.Value, cancellationToken);
        return Ok(MapTask(result.Value, assignee));
    }

    [HttpPost("api/v1/tasks/{taskId:int}/transition")]
    public async Task<IActionResult> TransitionAsync(int taskId, [FromBody] TaskTransitionRequest request, CancellationToken cancellationToken)
    {
        var result = await _tasks.TransitionAsync(taskId, request.ToStatus, cancellationToken);
        if (!result.IsSuccess || result.Value is null)
        {
            return result.ToActionResult(this, task => MapTask(task));
        }

        var assignee = await BuildAssigneeAsync(result.Value, cancellationToken);
        return Ok(MapTask(result.Value, assignee));
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

    private static TaskResponse MapTask(WorkTask task, TaskAssigneeResponse? assignedEmployee = null)
    {
        return new TaskResponse
        {
            Id = task.Id,
            ProjectId = task.ProjectId,
            AssignedEmployeeId = task.AssignedEmployeeId,
            AssignedEmployee = assignedEmployee,
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

    private async Task<TaskAssigneeResponse?> BuildAssigneeAsync(WorkTask task, CancellationToken cancellationToken)
    {
        if (!task.AssignedEmployeeId.HasValue)
        {
            return null;
        }

        var assignees = await BuildAssigneesAsync([task], cancellationToken);
        return ResolveAssignee(assignees, task);
    }

    private async Task<IReadOnlyDictionary<int, TaskAssigneeResponse>> BuildAssigneesAsync(
        IReadOnlyCollection<WorkTask> tasks,
        CancellationToken cancellationToken)
    {
        var employeeIds = tasks
            .Select(task => task.AssignedEmployeeId)
            .Where(id => id.HasValue)
            .Select(id => id!.Value)
            .Distinct()
            .ToArray();

        if (employeeIds.Length == 0)
        {
            return new Dictionary<int, TaskAssigneeResponse>();
        }

        var employees = await _employees.GetByIdsAsync(employeeIds, cancellationToken);
        if (employees.Count == 0)
        {
            return new Dictionary<int, TaskAssigneeResponse>();
        }

        var departments = await _departments.ListAsync(cancellationToken);
        var designations = await _designations.ListAsync(cancellationToken);

        var departmentNames = departments.ToDictionary(department => department.Id, department => department.Name);
        var designationNames = designations.ToDictionary(designation => designation.Id, designation => designation.Name);

        var result = new Dictionary<int, TaskAssigneeResponse>();
        foreach (var employee in employees)
        {
            departmentNames.TryGetValue(employee.DepartmentId, out var departmentName);
            designationNames.TryGetValue(employee.DesignationId, out var designationName);
            var fullName = string.Join(" ", new[] { employee.FirstName, employee.LastName }.Where(value => !string.IsNullOrWhiteSpace(value)));

            result[employee.Id] = new TaskAssigneeResponse
            {
                Id = employee.Id,
                Name = fullName,
                Department = departmentName ?? string.Empty,
                Designation = designationName ?? string.Empty
            };
        }

        return result;
    }

    private static TaskAssigneeResponse? ResolveAssignee(IReadOnlyDictionary<int, TaskAssigneeResponse> assignees, WorkTask task)
    {
        if (!task.AssignedEmployeeId.HasValue)
        {
            return null;
        }

        return assignees.TryGetValue(task.AssignedEmployeeId.Value, out var assignee) ? assignee : null;
    }
}
