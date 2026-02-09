using ProjectTaskStatus = Workforce.AppCore.Domain.Projects.TaskStatus;

namespace Workforce.AppCore.Abstractions.Queries;

public sealed class TaskQuery : PagedQuery
{
    public ProjectTaskStatus? Status { get; init; }
    public int? AssignedEmployeeId { get; init; }
}
