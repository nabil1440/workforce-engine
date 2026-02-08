using Workforce.AppCore.Domain.Projects;

namespace Workforce.AppCore.Abstractions.Queries;

public sealed class TaskQuery : PagedQuery
{
    public TaskStatus? Status { get; init; }
    public int? AssignedEmployeeId { get; init; }
}
