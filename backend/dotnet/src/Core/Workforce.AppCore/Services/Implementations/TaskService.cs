using Workforce.AppCore.Abstractions;
using Workforce.AppCore.Abstractions.Queries;
using Workforce.AppCore.Abstractions.Repositories;
using Workforce.AppCore.Abstractions.Results;
using Workforce.AppCore.Domain.Events;
using Workforce.AppCore.Domain.Projects;
using Workforce.AppCore.Validation;
using ProjectTaskStatus = Workforce.AppCore.Domain.Projects.TaskStatus;

namespace Workforce.AppCore.Services.Implementations;

public sealed class TaskService : ITaskService
{
    private readonly ITaskRepository _tasks;
    private readonly IEventPublisher _events;

    public TaskService(ITaskRepository tasks, IEventPublisher events)
    {
        _tasks = tasks;
        _events = events;
    }

    public async Task<Result<PagedResult<WorkTask>>> ListByProjectAsync(int projectId, TaskQuery query, CancellationToken cancellationToken = default)
    {
        var validation = Guard.Positive(projectId, nameof(projectId));
        if (!validation.IsSuccess)
        {
            return Result<PagedResult<WorkTask>>.Fail(validation.Error);
        }

        validation = Guard.NotNull(query, nameof(query));
        if (!validation.IsSuccess)
        {
            return Result<PagedResult<WorkTask>>.Fail(validation.Error);
        }

        var result = await _tasks.ListByProjectAsync(projectId, query, cancellationToken);
        return Result<PagedResult<WorkTask>>.Success(result);
    }

    public async Task<Result<WorkTask>> GetByIdAsync(int taskId, CancellationToken cancellationToken = default)
    {
        var validation = Guard.Positive(taskId, nameof(taskId));
        if (!validation.IsSuccess)
        {
            return Result<WorkTask>.Fail(validation.Error);
        }

        var task = await _tasks.GetByIdAsync(taskId, cancellationToken);
        return task is null
            ? Result<WorkTask>.Fail(Errors.NotFound("Task", taskId))
            : Result<WorkTask>.Success(task);
    }

    public async Task<Result<WorkTask>> CreateAsync(WorkTask task, CancellationToken cancellationToken = default)
    {
        var validation = TaskRules.Validate(task);
        if (!validation.IsSuccess)
        {
            return Result<WorkTask>.Fail(validation.Error);
        }

        var id = await _tasks.AddAsync(task, cancellationToken);
        task.Id = id;
        await _events.PublishAsync(new TaskCreated(task.Id, task.ProjectId, "system", null, task), cancellationToken);

        if (task.AssignedEmployeeId.HasValue)
        {
            await _events.PublishAsync(new TaskAssigned(task.Id, task.AssignedEmployeeId, "system", null, task), cancellationToken);
        }

        return Result<WorkTask>.Success(task);
    }

    public async Task<Result<WorkTask>> UpdateAsync(WorkTask task, CancellationToken cancellationToken = default)
    {
        var validation = TaskRules.Validate(task);
        if (!validation.IsSuccess)
        {
            return Result<WorkTask>.Fail(validation.Error);
        }

        if (task.Id <= 0)
        {
            return Result<WorkTask>.Fail(Errors.Invalid(nameof(task.Id), "must be greater than 0"));
        }

        var existing = await _tasks.GetByIdAsync(task.Id, cancellationToken);
        if (existing is null)
        {
            return Result<WorkTask>.Fail(Errors.NotFound("Task", task.Id));
        }

        await _tasks.UpdateAsync(task, cancellationToken);
        if (existing.AssignedEmployeeId != task.AssignedEmployeeId)
        {
            await _events.PublishAsync(new TaskAssigned(task.Id, task.AssignedEmployeeId, "system", existing, task), cancellationToken);
        }

        return Result<WorkTask>.Success(task);
    }

    public async Task<Result<WorkTask>> TransitionAsync(int taskId, ProjectTaskStatus toStatus, CancellationToken cancellationToken = default)
    {
        var validation = Guard.Positive(taskId, nameof(taskId));
        if (!validation.IsSuccess)
        {
            return Result<WorkTask>.Fail(validation.Error);
        }

        var task = await _tasks.GetByIdAsync(taskId, cancellationToken);
        if (task is null)
        {
            return Result<WorkTask>.Fail(Errors.NotFound("Task", taskId));
        }

        var before = new WorkTask
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
        validation = TaskRules.ValidateStatusChange(task.Status, toStatus);
        if (!validation.IsSuccess)
        {
            return Result<WorkTask>.Fail(validation.Error);
        }

        task.Status = toStatus;
        await _tasks.UpdateAsync(task, cancellationToken);
        await _events.PublishAsync(new TaskStatusChanged(task.Id, before.Status, toStatus, "system", before, task), cancellationToken);
        return Result<WorkTask>.Success(task);
    }
}
