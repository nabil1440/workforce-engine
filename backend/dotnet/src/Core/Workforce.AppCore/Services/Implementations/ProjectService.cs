using Workforce.AppCore.Abstractions;
using Workforce.AppCore.Abstractions.Queries;
using Workforce.AppCore.Abstractions.Repositories;
using Workforce.AppCore.Abstractions.Results;
using Workforce.AppCore.Domain.Events;
using Workforce.AppCore.Domain.Projects;
using Workforce.AppCore.Validation;

namespace Workforce.AppCore.Services.Implementations;

public sealed class ProjectService : IProjectService
{
    private readonly IProjectRepository _projects;
    private readonly IEventPublisher _events;

    public ProjectService(IProjectRepository projects, IEventPublisher events)
    {
        _projects = projects;
        _events = events;
    }

    public async Task<Result<PagedResult<Project>>> ListAsync(ProjectQuery query, CancellationToken cancellationToken = default)
    {
        var validation = Guard.NotNull(query, nameof(query));
        if (!validation.IsSuccess)
        {
            return Result<PagedResult<Project>>.Fail(validation.Error);
        }

        var result = await _projects.ListAsync(query, cancellationToken);
        return Result<PagedResult<Project>>.Success(result);
    }

    public async Task<Result<Project>> GetByIdAsync(int id, CancellationToken cancellationToken = default)
    {
        var validation = Guard.Positive(id, nameof(id));
        if (!validation.IsSuccess)
        {
            return Result<Project>.Fail(validation.Error);
        }

        var project = await _projects.GetByIdAsync(id, cancellationToken);
        return project is null
            ? Result<Project>.Fail(Errors.NotFound("Project", id))
            : Result<Project>.Success(project);
    }

    public async Task<Result<Project>> CreateAsync(Project project, CancellationToken cancellationToken = default)
    {
        var validation = ProjectRules.Validate(project);
        if (!validation.IsSuccess)
        {
            return Result<Project>.Fail(validation.Error);
        }

        var id = await _projects.AddAsync(project, cancellationToken);
        project.Id = id;
        await _events.PublishAsync(new ProjectCreated(project.Id, "system", null, project), cancellationToken);
        return Result<Project>.Success(project);
    }

    public async Task<Result<Project>> UpdateAsync(Project project, CancellationToken cancellationToken = default)
    {
        var validation = ProjectRules.Validate(project);
        if (!validation.IsSuccess)
        {
            return Result<Project>.Fail(validation.Error);
        }

        if (project.Id <= 0)
        {
            return Result<Project>.Fail(Errors.Invalid(nameof(project.Id), "must be greater than 0"));
        }

        var existing = await _projects.GetByIdAsync(project.Id, cancellationToken);
        if (existing is null)
        {
            return Result<Project>.Fail(Errors.NotFound("Project", project.Id));
        }

        await _projects.UpdateAsync(project, cancellationToken);
        await _events.PublishAsync(new ProjectUpdated(project.Id, "system", existing, project), cancellationToken);
        return Result<Project>.Success(project);
    }

    public async Task<Result<Project>> ChangeStatusAsync(int projectId, ProjectStatus toStatus, CancellationToken cancellationToken = default)
    {
        var validation = Guard.Positive(projectId, nameof(projectId));
        if (!validation.IsSuccess)
        {
            return Result<Project>.Fail(validation.Error);
        }

        var project = await _projects.GetByIdAsync(projectId, cancellationToken);
        if (project is null)
        {
            return Result<Project>.Fail(Errors.NotFound("Project", projectId));
        }

        validation = ProjectRules.ValidateStatusChange(project.Status, toStatus);
        if (!validation.IsSuccess)
        {
            return Result<Project>.Fail(validation.Error);
        }

        var before = new Project
        {
            Id = project.Id,
            Name = project.Name,
            Description = project.Description,
            Status = project.Status,
            StartDate = project.StartDate,
            EndDate = project.EndDate
        };
        project.Status = toStatus;
        await _projects.UpdateAsync(project, cancellationToken);
        await _events.PublishAsync(new ProjectStatusChanged(project.Id, before.Status, toStatus, "system", before, project), cancellationToken);
        return Result<Project>.Success(project);
    }
}
