using Workforce.AppCore.Abstractions.Queries;
using Workforce.AppCore.Domain.Projects;

namespace Workforce.AppCore.Abstractions.Repositories;

public interface ITaskRepository
{
    Task<PagedResult<WorkTask>> ListByProjectAsync(int projectId, TaskQuery query, CancellationToken cancellationToken = default);
    Task<WorkTask?> GetByIdAsync(int taskId, CancellationToken cancellationToken = default);
    Task<int> AddAsync(WorkTask task, CancellationToken cancellationToken = default);
    Task UpdateAsync(WorkTask task, CancellationToken cancellationToken = default);
}
