using Microsoft.EntityFrameworkCore;
using Workforce.AppCore.Abstractions.Queries;
using Workforce.AppCore.Abstractions.Repositories;
using Workforce.AppCore.Domain.Projects;

namespace Workforce.Infrastructure.Sql.Repositories;

public sealed class ProjectRepository : IProjectRepository
{
    private readonly WorkforceDbContext _dbContext;

    public ProjectRepository(WorkforceDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<PagedResult<Project>> ListAsync(ProjectQuery query, CancellationToken cancellationToken = default)
    {
        var normalized = NormalizePaging(query.Page, query.PageSize);
        var projects = _dbContext.Projects.AsNoTracking().AsQueryable();

        if (query.Status.HasValue)
        {
            projects = projects.Where(p => p.Status == query.Status.Value);
        }

        if (!string.IsNullOrWhiteSpace(query.Search))
        {
            var term = query.Search.Trim().ToLower();
            projects = projects.Where(p => p.Name.ToLower().Contains(term));
        }

        projects = ApplySorting(projects, query.SortBy, query.SortDirection);

        var totalCount = await projects.CountAsync(cancellationToken);
        var items = await projects
            .Skip((normalized.page - 1) * normalized.pageSize)
            .Take(normalized.pageSize)
            .ToListAsync(cancellationToken);

        return new PagedResult<Project>
        {
            Items = items,
            TotalCount = totalCount,
            Page = normalized.page,
            PageSize = normalized.pageSize
        };
    }

    public Task<Project?> GetByIdAsync(int id, CancellationToken cancellationToken = default)
    {
        return _dbContext.Projects.AsNoTracking().FirstOrDefaultAsync(p => p.Id == id, cancellationToken);
    }

    public async Task<int> AddAsync(Project project, CancellationToken cancellationToken = default)
    {
        _dbContext.Projects.Add(project);
        await _dbContext.SaveChangesAsync(cancellationToken);
        return project.Id;
    }

    public async Task UpdateAsync(Project project, CancellationToken cancellationToken = default)
    {
        _dbContext.Projects.Update(project);
        await _dbContext.SaveChangesAsync(cancellationToken);
    }

    private static IQueryable<Project> ApplySorting(IQueryable<Project> queryable, string? sortBy, SortDirection direction)
    {
        var key = sortBy?.Trim().ToLowerInvariant();
        var ascending = direction == SortDirection.Asc;

        return key switch
        {
            "name" => ascending ? queryable.OrderBy(p => p.Name) : queryable.OrderByDescending(p => p.Name),
            "status" => ascending ? queryable.OrderBy(p => p.Status) : queryable.OrderByDescending(p => p.Status),
            "startdate" => ascending ? queryable.OrderBy(p => p.StartDate) : queryable.OrderByDescending(p => p.StartDate),
            "enddate" => ascending ? queryable.OrderBy(p => p.EndDate) : queryable.OrderByDescending(p => p.EndDate),
            _ => ascending ? queryable.OrderBy(p => p.Id) : queryable.OrderByDescending(p => p.Id)
        };
    }

    private static (int page, int pageSize) NormalizePaging(int page, int pageSize)
    {
        var safePage = page <= 0 ? 1 : page;
        var safePageSize = pageSize <= 0 ? 20 : pageSize;
        return (safePage, safePageSize);
    }
}
