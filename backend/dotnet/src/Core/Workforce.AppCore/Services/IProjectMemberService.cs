using Workforce.AppCore.Abstractions.Results;
using Workforce.AppCore.Domain.Projects;

namespace Workforce.AppCore.Services;

public interface IProjectMemberService
{
    Task<Result> AddMemberAsync(int projectId, int employeeId, CancellationToken cancellationToken = default);
    Task<Result> RemoveMemberAsync(int projectId, int employeeId, CancellationToken cancellationToken = default);
    Task<Result<IReadOnlyList<ProjectMember>>> ListMembersAsync(int projectId, CancellationToken cancellationToken = default);
}
