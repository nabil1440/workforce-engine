using ProjectTaskStatus = Workforce.AppCore.Domain.Projects.TaskStatus;
using Workforce.AppCore.Abstractions.Queries;

namespace Workforce.Api.Contracts.Queries;

public sealed class TaskListQuery
{
    public int Page { get; init; } = 1;
    public int PageSize { get; init; } = 20;
    public ProjectTaskStatus? Status { get; init; }
    public int? AssignedEmployeeId { get; init; }
    public string? Sort { get; init; }
    public SortDirection SortDirection { get; init; } = SortDirection.Asc;
}
