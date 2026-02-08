using Workforce.AppCore.Domain.Projects;

namespace Workforce.AppCore.Services;

public interface IProjectMemberService
{
    Task AddMemberAsync(int projectId, int employeeId, CancellationToken cancellationToken = default);
    Task RemoveMemberAsync(int projectId, int employeeId, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<ProjectMember>> ListMembersAsync(int projectId, CancellationToken cancellationToken = default);
}
