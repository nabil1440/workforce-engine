using Microsoft.AspNetCore.Mvc;
using Workforce.AppCore.Abstractions.Repositories;
using Workforce.AppCore.Domain.Employees;

namespace Workforce.Api.Controllers;

[ApiController]
[Route("api/v1/departments")]
public sealed class DepartmentsController : ControllerBase
{
    private readonly IDepartmentRepository _departments;

    public DepartmentsController(IDepartmentRepository departments)
    {
        _departments = departments;
    }

    [HttpGet]
    public async Task<ActionResult<IReadOnlyList<Department>>> ListAsync(CancellationToken cancellationToken)
    {
        var items = await _departments.ListAsync(cancellationToken);
        return Ok(items);
    }
}
