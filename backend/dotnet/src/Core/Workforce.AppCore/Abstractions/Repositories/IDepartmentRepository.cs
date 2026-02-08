using Workforce.AppCore.Domain.Employees;

namespace Workforce.AppCore.Abstractions.Repositories;

public interface IDepartmentRepository
{
    Task<IReadOnlyList<Department>> ListAsync(CancellationToken cancellationToken = default);
}
