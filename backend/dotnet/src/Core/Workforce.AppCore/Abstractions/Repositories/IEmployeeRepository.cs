using Workforce.AppCore.Abstractions.Queries;
using Workforce.AppCore.Domain.Employees;

namespace Workforce.AppCore.Abstractions.Repositories;

public interface IEmployeeRepository
{
    Task<PagedResult<Employee>> ListAsync(EmployeeQuery query, CancellationToken cancellationToken = default);
    Task<Employee?> GetByIdAsync(int id, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<Employee>> GetByIdsAsync(IReadOnlyCollection<int> ids, CancellationToken cancellationToken = default);
    Task<int> AddAsync(Employee employee, CancellationToken cancellationToken = default);
    Task UpdateAsync(Employee employee, CancellationToken cancellationToken = default);
    Task DeactivateAsync(int id, CancellationToken cancellationToken = default);
}
