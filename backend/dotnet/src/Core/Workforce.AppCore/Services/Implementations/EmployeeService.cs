using Workforce.AppCore.Abstractions;
using Workforce.AppCore.Abstractions.Queries;
using Workforce.AppCore.Abstractions.Repositories;
using Workforce.AppCore.Abstractions.Results;
using Workforce.AppCore.Domain.Events;
using Workforce.AppCore.Domain.Employees;
using Workforce.AppCore.Validation;

namespace Workforce.AppCore.Services.Implementations;

public sealed class EmployeeService : IEmployeeService
{
    private readonly IEmployeeRepository _employees;
    private readonly IEventPublisher _events;

    public EmployeeService(IEmployeeRepository employees, IEventPublisher events)
    {
        _employees = employees;
        _events = events;
    }

    public async Task<Result<PagedResult<Employee>>> ListAsync(EmployeeQuery query, CancellationToken cancellationToken = default)
    {
        var validation = Guard.NotNull(query, nameof(query));
        if (!validation.IsSuccess)
        {
            return Result<PagedResult<Employee>>.Fail(validation.Error);
        }

        var result = await _employees.ListAsync(query, cancellationToken);
        return Result<PagedResult<Employee>>.Success(result);
    }

    public async Task<Result<Employee>> GetByIdAsync(int id, CancellationToken cancellationToken = default)
    {
        var validation = Guard.Positive(id, nameof(id));
        if (!validation.IsSuccess)
        {
            return Result<Employee>.Fail(validation.Error);
        }

        var employee = await _employees.GetByIdAsync(id, cancellationToken);
        return employee is null
            ? Result<Employee>.Fail(Errors.NotFound("Employee", id))
            : Result<Employee>.Success(employee);
    }

    public async Task<Result<Employee>> CreateAsync(Employee employee, CancellationToken cancellationToken = default)
    {
        var validation = EmployeeRules.Validate(employee);
        if (!validation.IsSuccess)
        {
            return Result<Employee>.Fail(validation.Error);
        }

        var id = await _employees.AddAsync(employee, cancellationToken);
        employee.Id = id;
        await _events.PublishAsync(new EmployeeCreated(employee.Id, "system", null, employee), cancellationToken);
        return Result<Employee>.Success(employee);
    }

    public async Task<Result<Employee>> UpdateAsync(Employee employee, CancellationToken cancellationToken = default)
    {
        var validation = EmployeeRules.Validate(employee);
        if (!validation.IsSuccess)
        {
            return Result<Employee>.Fail(validation.Error);
        }

        if (employee.Id <= 0)
        {
            return Result<Employee>.Fail(Errors.Invalid(nameof(employee.Id), "must be greater than 0"));
        }

        var existing = await _employees.GetByIdAsync(employee.Id, cancellationToken);
        if (existing is null)
        {
            return Result<Employee>.Fail(Errors.NotFound("Employee", employee.Id));
        }

        await _employees.UpdateAsync(employee, cancellationToken);
        await _events.PublishAsync(new EmployeeUpdated(employee.Id, "system", existing, employee), cancellationToken);
        return Result<Employee>.Success(employee);
    }

    public async Task<Result> DeactivateAsync(int id, CancellationToken cancellationToken = default)
    {
        var validation = Guard.Positive(id, nameof(id));
        if (!validation.IsSuccess)
        {
            return validation;
        }

        var existing = await _employees.GetByIdAsync(id, cancellationToken);
        if (existing is null)
        {
            return Result.Fail(Errors.NotFound("Employee", id));
        }

        await _employees.DeactivateAsync(id, cancellationToken);
        var after = new Employee
        {
            Id = existing.Id,
            FirstName = existing.FirstName,
            LastName = existing.LastName,
            Email = existing.Email,
            IsActive = false,
            DepartmentId = existing.DepartmentId,
            DesignationId = existing.DesignationId,
            Salary = existing.Salary,
            JoiningDate = existing.JoiningDate,
            Phone = existing.Phone,
            Address = existing.Address,
            City = existing.City,
            Country = existing.Country
        };
        await _events.PublishAsync(new EmployeeDeactivated(id, "system", existing, after), cancellationToken);
        return Result.Success();
    }
}
