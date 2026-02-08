namespace Workforce.AppCore.Abstractions.Queries;

public sealed class EmployeeQuery : PagedQuery
{
    public int? DepartmentId { get; init; }
    public bool? IsActive { get; init; }
    public string? Search { get; init; }
}
