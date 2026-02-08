using Workforce.AppCore.Abstractions.Queries;
using Workforce.AppCore.Abstractions.Results;
using Workforce.AppCore.Domain.Employees;

namespace Workforce.AppCore.Services;

public interface IEmployeeService
{
    Task<Result<PagedResult<Employee>>> ListAsync(EmployeeQuery query, CancellationToken cancellationToken = default);
    Task<Result<Employee>> GetByIdAsync(int id, CancellationToken cancellationToken = default);
    Task<Result<Employee>> CreateAsync(Employee employee, CancellationToken cancellationToken = default);
    Task<Result<Employee>> UpdateAsync(Employee employee, CancellationToken cancellationToken = default);
    Task<Result> DeactivateAsync(int id, CancellationToken cancellationToken = default);
}
