using Workforce.AppCore.Abstractions.Repositories;
using Workforce.AppCore.Abstractions.Results;
using Workforce.AppCore.Domain.Projects;
using Workforce.AppCore.Validation;

namespace Workforce.AppCore.Services.Implementations;

public sealed class ProjectMemberService : IProjectMemberService
{
    private readonly IProjectMemberRepository _members;

    public ProjectMemberService(IProjectMemberRepository members)
    {
        _members = members;
    }

    public async Task<Result> AddMemberAsync(int projectId, int employeeId, CancellationToken cancellationToken = default)
    {
        var validation = Guard.Positive(projectId, nameof(projectId));
        if (!validation.IsSuccess)
        {
            return validation;
        }

        validation = Guard.Positive(employeeId, nameof(employeeId));
        if (!validation.IsSuccess)
        {
            return validation;
        }

        await _members.AddAsync(projectId, employeeId, cancellationToken);
        return Result.Success();
    }

    public async Task<Result> RemoveMemberAsync(int projectId, int employeeId, CancellationToken cancellationToken = default)
    {
        var validation = Guard.Positive(projectId, nameof(projectId));
        if (!validation.IsSuccess)
        {
            return validation;
        }

        validation = Guard.Positive(employeeId, nameof(employeeId));
        if (!validation.IsSuccess)
        {
            return validation;
        }

        await _members.RemoveAsync(projectId, employeeId, cancellationToken);
        return Result.Success();
    }

    public async Task<Result<IReadOnlyList<ProjectMember>>> ListMembersAsync(int projectId, CancellationToken cancellationToken = default)
    {
        var validation = Guard.Positive(projectId, nameof(projectId));
        if (!validation.IsSuccess)
        {
            return Result<IReadOnlyList<ProjectMember>>.Fail(validation.Error);
        }

        var members = await _members.ListByProjectAsync(projectId, cancellationToken);
        return Result<IReadOnlyList<ProjectMember>>.Success(members);
    }
}
