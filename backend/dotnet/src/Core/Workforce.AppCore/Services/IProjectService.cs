using Workforce.AppCore.Abstractions.Queries;
using Workforce.AppCore.Domain.Projects;

namespace Workforce.AppCore.Services;

public interface IProjectService
{
    Task<PagedResult<Project>> ListAsync(ProjectQuery query, CancellationToken cancellationToken = default);
    Task<Project?> GetByIdAsync(int id, CancellationToken cancellationToken = default);
    Task<Project> CreateAsync(Project project, CancellationToken cancellationToken = default);
    Task<Project> UpdateAsync(Project project, CancellationToken cancellationToken = default);
    Task<Project> ChangeStatusAsync(int projectId, ProjectStatus toStatus, CancellationToken cancellationToken = default);
}
