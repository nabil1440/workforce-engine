using Microsoft.EntityFrameworkCore;
using Workforce.AppCore.Abstractions.Repositories;
using Workforce.AppCore.Domain.Employees;

namespace Workforce.Infrastructure.Sql.Repositories;

public sealed class DepartmentRepository : IDepartmentRepository
{
    private readonly WorkforceDbContext _dbContext;

    public DepartmentRepository(WorkforceDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<IReadOnlyList<Department>> ListAsync(CancellationToken cancellationToken = default)
    {
        return await _dbContext.Departments
            .AsNoTracking()
            .OrderBy(d => d.Name)
            .ToListAsync(cancellationToken);
    }
}
