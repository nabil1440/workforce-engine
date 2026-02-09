using Microsoft.EntityFrameworkCore;
using Workforce.AppCore.Abstractions.Queries;
using Workforce.AppCore.Abstractions.Repositories;
using Workforce.AppCore.Domain.Employees;

namespace Workforce.Infrastructure.Sql.Repositories;

public sealed class EmployeeRepository : IEmployeeRepository
{
    private readonly WorkforceDbContext _dbContext;

    public EmployeeRepository(WorkforceDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<PagedResult<Employee>> ListAsync(EmployeeQuery query, CancellationToken cancellationToken = default)
    {
        var normalized = NormalizePaging(query.Page, query.PageSize);
        var employees = _dbContext.Employees.AsNoTracking().AsQueryable();

        if (query.DepartmentId.HasValue)
        {
            employees = employees.Where(e => e.DepartmentId == query.DepartmentId.Value);
        }

        if (query.IsActive.HasValue)
        {
            employees = employees.Where(e => e.IsActive == query.IsActive.Value);
        }

        if (!string.IsNullOrWhiteSpace(query.Search))
        {
            var term = query.Search.Trim().ToLower();
            employees = employees.Where(e =>
                e.FirstName.ToLower().Contains(term) ||
                e.LastName.ToLower().Contains(term) ||
                e.Email.ToLower().Contains(term));
        }

        employees = ApplySorting(employees, query.SortBy, query.SortDirection);

        var totalCount = await employees.CountAsync(cancellationToken);
        var items = await employees
            .Skip((normalized.page - 1) * normalized.pageSize)
            .Take(normalized.pageSize)
            .ToListAsync(cancellationToken);

        return new PagedResult<Employee>
        {
            Items = items,
            TotalCount = totalCount,
            Page = normalized.page,
            PageSize = normalized.pageSize
        };
    }

    public Task<Employee?> GetByIdAsync(int id, CancellationToken cancellationToken = default)
    {
        return _dbContext.Employees.AsNoTracking().FirstOrDefaultAsync(e => e.Id == id, cancellationToken);
    }

    public async Task<IReadOnlyList<Employee>> GetByIdsAsync(IReadOnlyCollection<int> ids, CancellationToken cancellationToken = default)
    {
        if (ids.Count == 0)
        {
            return Array.Empty<Employee>();
        }

        return await _dbContext.Employees.AsNoTracking()
            .Where(e => ids.Contains(e.Id))
            .ToListAsync(cancellationToken);
    }

    public async Task<int> AddAsync(Employee employee, CancellationToken cancellationToken = default)
    {
        _dbContext.Employees.Add(employee);
        await _dbContext.SaveChangesAsync(cancellationToken);
        return employee.Id;
    }

    public async Task UpdateAsync(Employee employee, CancellationToken cancellationToken = default)
    {
        _dbContext.Employees.Update(employee);
        await _dbContext.SaveChangesAsync(cancellationToken);
    }

    public async Task DeactivateAsync(int id, CancellationToken cancellationToken = default)
    {
        var employee = await _dbContext.Employees.FirstOrDefaultAsync(e => e.Id == id, cancellationToken);
        if (employee is null)
        {
            return;
        }

        employee.IsActive = false;
        await _dbContext.SaveChangesAsync(cancellationToken);
    }

    private static IQueryable<Employee> ApplySorting(IQueryable<Employee> queryable, string? sortBy, SortDirection direction)
    {
        var key = sortBy?.Trim().ToLowerInvariant();
        var ascending = direction == SortDirection.Asc;

        return key switch
        {
            "firstname" => ascending ? queryable.OrderBy(e => e.FirstName) : queryable.OrderByDescending(e => e.FirstName),
            "lastname" => ascending ? queryable.OrderBy(e => e.LastName) : queryable.OrderByDescending(e => e.LastName),
            "email" => ascending ? queryable.OrderBy(e => e.Email) : queryable.OrderByDescending(e => e.Email),
            "joiningdate" => ascending ? queryable.OrderBy(e => e.JoiningDate) : queryable.OrderByDescending(e => e.JoiningDate),
            "salary" => ascending ? queryable.OrderBy(e => e.Salary) : queryable.OrderByDescending(e => e.Salary),
            _ => ascending ? queryable.OrderBy(e => e.Id) : queryable.OrderByDescending(e => e.Id)
        };
    }

    private static (int page, int pageSize) NormalizePaging(int page, int pageSize)
    {
        var safePage = page <= 0 ? 1 : page;
        var safePageSize = pageSize <= 0 ? 20 : pageSize;
        return (safePage, safePageSize);
    }
}
