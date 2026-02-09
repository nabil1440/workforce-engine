using Microsoft.EntityFrameworkCore;
using Workforce.AppCore.Abstractions.Repositories;
using Workforce.AppCore.Domain.Projects;

namespace Workforce.Infrastructure.Sql.Repositories;

public sealed class ProjectMemberRepository : IProjectMemberRepository
{
    private readonly WorkforceDbContext _dbContext;

    public ProjectMemberRepository(WorkforceDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task AddAsync(int projectId, int employeeId, CancellationToken cancellationToken = default)
    {
        var entity = new ProjectMember { ProjectId = projectId, EmployeeId = employeeId };
        _dbContext.ProjectMembers.Add(entity);
        await _dbContext.SaveChangesAsync(cancellationToken);
    }

    public async Task RemoveAsync(int projectId, int employeeId, CancellationToken cancellationToken = default)
    {
        var entity = await _dbContext.ProjectMembers
            .FirstOrDefaultAsync(pm => pm.ProjectId == projectId && pm.EmployeeId == employeeId, cancellationToken);

        if (entity is null)
        {
            return;
        }

        _dbContext.ProjectMembers.Remove(entity);
        await _dbContext.SaveChangesAsync(cancellationToken);
    }

    public async Task<IReadOnlyList<ProjectMember>> ListByProjectAsync(int projectId, CancellationToken cancellationToken = default)
    {
        return await _dbContext.ProjectMembers
            .AsNoTracking()
            .Where(pm => pm.ProjectId == projectId)
            .ToListAsync(cancellationToken);
    }
}
