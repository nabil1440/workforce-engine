using Workforce.AppCore.Abstractions.Queries;
using Workforce.AppCore.Domain.Projects;

namespace Workforce.AppCore.Abstractions.Repositories;

public interface IProjectRepository
{
    Task<PagedResult<Project>> ListAsync(ProjectQuery query, CancellationToken cancellationToken = default);
    Task<Project?> GetByIdAsync(int id, CancellationToken cancellationToken = default);
    Task<int> AddAsync(Project project, CancellationToken cancellationToken = default);
    Task UpdateAsync(Project project, CancellationToken cancellationToken = default);
}
