using ProjectTaskStatus = Workforce.AppCore.Domain.Projects.TaskStatus;
using Workforce.AppCore.Domain.Projects;

namespace Workforce.Api.Contracts.Tasks;

public sealed class TaskResponse
{
    public int Id { get; init; }
    public int ProjectId { get; init; }
    public int? AssignedEmployeeId { get; init; }
    public string Title { get; init; } = string.Empty;
    public string? Description { get; init; }
    public ProjectTaskStatus Status { get; init; }
    public TaskPriority Priority { get; init; }
    public DateOnly DueDate { get; init; }
}
