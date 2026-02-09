using Microsoft.EntityFrameworkCore;
using Workforce.AppCore.Abstractions.Queries;
using Workforce.AppCore.Abstractions.Repositories;
using Workforce.AppCore.Domain.Projects;

namespace Workforce.Infrastructure.Sql.Repositories;

public sealed class TaskRepository : ITaskRepository
{
    private readonly WorkforceDbContext _dbContext;

    public TaskRepository(WorkforceDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<PagedResult<WorkTask>> ListByProjectAsync(int projectId, TaskQuery query, CancellationToken cancellationToken = default)
    {
        var normalized = NormalizePaging(query.Page, query.PageSize);
        var tasks = _dbContext.Tasks.AsNoTracking().Where(t => t.ProjectId == projectId).AsQueryable();

        if (query.Status.HasValue)
        {
            tasks = tasks.Where(t => t.Status == query.Status.Value);
        }

        if (query.AssignedEmployeeId.HasValue)
        {
            tasks = tasks.Where(t => t.AssignedEmployeeId == query.AssignedEmployeeId.Value);
        }

        tasks = ApplySorting(tasks, query.SortBy, query.SortDirection);

        var totalCount = await tasks.CountAsync(cancellationToken);
        var items = await tasks
            .Skip((normalized.page - 1) * normalized.pageSize)
            .Take(normalized.pageSize)
            .ToListAsync(cancellationToken);

        return new PagedResult<WorkTask>
        {
            Items = items,
            TotalCount = totalCount,
            Page = normalized.page,
            PageSize = normalized.pageSize
        };
    }

    public Task<WorkTask?> GetByIdAsync(int taskId, CancellationToken cancellationToken = default)
    {
        return _dbContext.Tasks.AsNoTracking().FirstOrDefaultAsync(t => t.Id == taskId, cancellationToken);
    }

    public async Task<int> AddAsync(WorkTask task, CancellationToken cancellationToken = default)
    {
        _dbContext.Tasks.Add(task);
        await _dbContext.SaveChangesAsync(cancellationToken);
        return task.Id;
    }

    public async Task UpdateAsync(WorkTask task, CancellationToken cancellationToken = default)
    {
        _dbContext.Tasks.Update(task);
        await _dbContext.SaveChangesAsync(cancellationToken);
    }

    private static IQueryable<WorkTask> ApplySorting(IQueryable<WorkTask> queryable, string? sortBy, SortDirection direction)
    {
        var key = sortBy?.Trim().ToLowerInvariant();
        var ascending = direction == SortDirection.Asc;

        return key switch
        {
            "title" => ascending ? queryable.OrderBy(t => t.Title) : queryable.OrderByDescending(t => t.Title),
            "status" => ascending ? queryable.OrderBy(t => t.Status) : queryable.OrderByDescending(t => t.Status),
            "priority" => ascending ? queryable.OrderBy(t => t.Priority) : queryable.OrderByDescending(t => t.Priority),
            "duedate" => ascending ? queryable.OrderBy(t => t.DueDate) : queryable.OrderByDescending(t => t.DueDate),
            _ => ascending ? queryable.OrderBy(t => t.Id) : queryable.OrderByDescending(t => t.Id)
        };
    }

    private static (int page, int pageSize) NormalizePaging(int page, int pageSize)
    {
        var safePage = page <= 0 ? 1 : page;
        var safePageSize = pageSize <= 0 ? 20 : pageSize;
        return (safePage, safePageSize);
    }
}
