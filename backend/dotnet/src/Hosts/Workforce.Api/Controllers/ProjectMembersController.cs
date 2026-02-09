using Microsoft.AspNetCore.Mvc;
using Workforce.Api.Contracts.Common;
using Workforce.Api.Contracts.Projects;
using Workforce.AppCore.Services;

namespace Workforce.Api.Controllers;

[ApiController]
[Route("api/v1/projects/{id:int}/members")]
public sealed class ProjectMembersController : ControllerBase
{
    private readonly IProjectMemberService _members;

    public ProjectMembersController(IProjectMemberService members)
    {
        _members = members;
    }

    [HttpPost]
    public async Task<IActionResult> AddMemberAsync(int id, [FromBody] ProjectMemberRequest request, CancellationToken cancellationToken)
    {
        var result = await _members.AddMemberAsync(id, request.EmployeeId, cancellationToken);
        return result.ToActionResult(this);
    }

    [HttpDelete("{employeeId:int}")]
    public async Task<IActionResult> RemoveMemberAsync(int id, int employeeId, CancellationToken cancellationToken)
    {
        var result = await _members.RemoveMemberAsync(id, employeeId, cancellationToken);
        return result.ToActionResult(this);
    }
}
