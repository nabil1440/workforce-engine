using Workforce.AppCore.Abstractions.Queries;
using Workforce.AppCore.Domain.Projects;

namespace Workforce.AppCore.Services;

public interface ITaskService
{
    Task<PagedResult<WorkTask>> ListByProjectAsync(int projectId, TaskQuery query, CancellationToken cancellationToken = default);
    Task<WorkTask?> GetByIdAsync(int taskId, CancellationToken cancellationToken = default);
    Task<WorkTask> CreateAsync(WorkTask task, CancellationToken cancellationToken = default);
    Task<WorkTask> UpdateAsync(WorkTask task, CancellationToken cancellationToken = default);
    Task<WorkTask> TransitionAsync(int taskId, TaskStatus toStatus, CancellationToken cancellationToken = default);
}
