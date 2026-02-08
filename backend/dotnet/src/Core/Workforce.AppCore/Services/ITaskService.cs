using Workforce.AppCore.Abstractions.Queries;
using Workforce.AppCore.Abstractions.Results;
using ProjectTaskStatus = Workforce.AppCore.Domain.Projects.TaskStatus;
using Workforce.AppCore.Domain.Projects;

namespace Workforce.AppCore.Services;

public interface ITaskService
{
    Task<Result<PagedResult<WorkTask>>> ListByProjectAsync(int projectId, TaskQuery query, CancellationToken cancellationToken = default);
    Task<Result<WorkTask>> GetByIdAsync(int taskId, CancellationToken cancellationToken = default);
    Task<Result<WorkTask>> CreateAsync(WorkTask task, CancellationToken cancellationToken = default);
    Task<Result<WorkTask>> UpdateAsync(WorkTask task, CancellationToken cancellationToken = default);
    Task<Result<WorkTask>> TransitionAsync(int taskId, ProjectTaskStatus toStatus, CancellationToken cancellationToken = default);
}
