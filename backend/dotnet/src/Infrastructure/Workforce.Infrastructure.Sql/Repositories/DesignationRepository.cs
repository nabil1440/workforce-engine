using Microsoft.EntityFrameworkCore;
using Workforce.AppCore.Abstractions.Repositories;
using Workforce.AppCore.Domain.Employees;

namespace Workforce.Infrastructure.Sql.Repositories;

public sealed class DesignationRepository : IDesignationRepository
{
    private readonly WorkforceDbContext _dbContext;

    public DesignationRepository(WorkforceDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<IReadOnlyList<Designation>> ListAsync(CancellationToken cancellationToken = default)
    {
        return await _dbContext.Designations
            .AsNoTracking()
            .OrderBy(d => d.Name)
            .ToListAsync(cancellationToken);
    }
}
