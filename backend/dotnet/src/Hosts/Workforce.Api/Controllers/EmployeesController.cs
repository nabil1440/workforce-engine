using Microsoft.AspNetCore.Mvc;
using Workforce.Api.Contracts.Common;
using Workforce.Api.Contracts.Employees;
using Workforce.Api.Contracts.Queries;
using Workforce.AppCore.Abstractions.Queries;
using Workforce.AppCore.Domain.Employees;
using Workforce.AppCore.Services;

namespace Workforce.Api.Controllers;

[ApiController]
[Route("api/v1/employees")]
public sealed class EmployeesController : ControllerBase
{
    private readonly IEmployeeService _employees;

    public EmployeesController(IEmployeeService employees)
    {
        _employees = employees;
    }

    [HttpGet]
    public async Task<IActionResult> ListAsync(
        [FromQuery] EmployeeListQuery request,
        CancellationToken cancellationToken = default)
    {
        var query = new EmployeeQuery
        {
            Page = request.Page,
            PageSize = request.PageSize,
            DepartmentId = request.DepartmentId,
            IsActive = request.IsActive,
            Search = request.Search,
            SortBy = request.Sort,
            SortDirection = request.SortDirection
        };

        var result = await _employees.ListAsync(query, cancellationToken);
        return result.ToActionResult(this, value => MapPaged(value, MapEmployee));
    }

    [HttpGet("{id:int}")]
    public async Task<IActionResult> GetByIdAsync(int id, CancellationToken cancellationToken)
    {
        var result = await _employees.GetByIdAsync(id, cancellationToken);
        return result.ToActionResult(this, MapEmployee);
    }

    [HttpPost]
    public async Task<IActionResult> CreateAsync([FromBody] EmployeeRequest request, CancellationToken cancellationToken)
    {
        var employee = MapEmployee(request);
        var result = await _employees.CreateAsync(employee, cancellationToken);

        if (!result.IsSuccess || result.Value is null)
        {
            return result.ToActionResult(this);
        }

        return CreatedAtAction(nameof(GetByIdAsync), new { id = result.Value.Id }, MapEmployee(result.Value));
    }

    [HttpPut("{id:int}")]
    public async Task<IActionResult> UpdateAsync(int id, [FromBody] EmployeeRequest request, CancellationToken cancellationToken)
    {
        var employee = MapEmployee(request);
        employee.Id = id;
        var result = await _employees.UpdateAsync(employee, cancellationToken);
        return result.ToActionResult(this, MapEmployee);
    }

    [HttpDelete("{id:int}")]
    public async Task<IActionResult> DeactivateAsync(int id, CancellationToken cancellationToken)
    {
        var result = await _employees.DeactivateAsync(id, cancellationToken);
        return result.ToActionResult(this);
    }

    private static Employee MapEmployee(EmployeeRequest request)
    {
        return new Employee
        {
            FirstName = request.FirstName,
            LastName = request.LastName,
            Email = request.Email,
            DepartmentId = request.DepartmentId,
            DesignationId = request.DesignationId,
            Salary = request.Salary,
            JoiningDate = request.JoiningDate,
            Phone = request.Phone,
            Address = request.Address,
            City = request.City,
            Country = request.Country,
            IsActive = request.IsActive
        };
    }

    private static EmployeeResponse MapEmployee(Employee employee)
    {
        return new EmployeeResponse
        {
            Id = employee.Id,
            FirstName = employee.FirstName,
            LastName = employee.LastName,
            Email = employee.Email,
            IsActive = employee.IsActive,
            DepartmentId = employee.DepartmentId,
            DesignationId = employee.DesignationId,
            Salary = employee.Salary,
            JoiningDate = employee.JoiningDate,
            Phone = employee.Phone,
            Address = employee.Address,
            City = employee.City,
            Country = employee.Country
        };
    }

    private static PagedResponse<EmployeeResponse> MapPaged(PagedResult<Employee> pagedResult, Func<Employee, EmployeeResponse> map)
    {
        return new PagedResponse<EmployeeResponse>
        {
            Items = pagedResult.Items.Select(map).ToList(),
            TotalCount = pagedResult.TotalCount,
            Page = pagedResult.Page,
            PageSize = pagedResult.PageSize
        };
    }
}
