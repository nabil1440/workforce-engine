using Microsoft.AspNetCore.Mvc;
using Workforce.AppCore.Abstractions.Repositories;
using Workforce.AppCore.Domain.Employees;

namespace Workforce.Api.Controllers;

[ApiController]
[Route("api/v1/designations")]
public sealed class DesignationsController : ControllerBase
{
    private readonly IDesignationRepository _designations;

    public DesignationsController(IDesignationRepository designations)
    {
        _designations = designations;
    }

    [HttpGet]
    public async Task<ActionResult<IReadOnlyList<Designation>>> ListAsync(CancellationToken cancellationToken)
    {
        var items = await _designations.ListAsync(cancellationToken);
        return Ok(items);
    }
}
