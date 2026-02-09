using Workforce.AppCore.Domain.Projects;

namespace Workforce.AppCore.Abstractions.Repositories;

public interface IProjectMemberRepository
{
    Task AddAsync(int projectId, int employeeId, CancellationToken cancellationToken = default);
    Task RemoveAsync(int projectId, int employeeId, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<ProjectMember>> ListByProjectAsync(int projectId, CancellationToken cancellationToken = default);
}
