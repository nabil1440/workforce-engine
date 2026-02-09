using Workforce.AppCore.Abstractions.Queries;
using Workforce.AppCore.Abstractions.Results;
using Workforce.AppCore.Domain.Projects;

namespace Workforce.AppCore.Services;

public interface IProjectService
{
    Task<Result<PagedResult<Project>>> ListAsync(ProjectQuery query, CancellationToken cancellationToken = default);
    Task<Result<Project>> GetByIdAsync(int id, CancellationToken cancellationToken = default);
    Task<Result<Project>> CreateAsync(Project project, CancellationToken cancellationToken = default);
    Task<Result<Project>> UpdateAsync(Project project, CancellationToken cancellationToken = default);
    Task<Result<Project>> ChangeStatusAsync(int projectId, ProjectStatus toStatus, CancellationToken cancellationToken = default);
}
