using Workforce.AppCore.Abstractions.Queries;
using Workforce.AppCore.Domain.Employees;

namespace Workforce.AppCore.Services;

public interface IEmployeeService
{
    Task<PagedResult<Employee>> ListAsync(EmployeeQuery query, CancellationToken cancellationToken = default);
    Task<Employee?> GetByIdAsync(int id, CancellationToken cancellationToken = default);
    Task<Employee> CreateAsync(Employee employee, CancellationToken cancellationToken = default);
    Task<Employee> UpdateAsync(Employee employee, CancellationToken cancellationToken = default);
    Task DeactivateAsync(int id, CancellationToken cancellationToken = default);
}
