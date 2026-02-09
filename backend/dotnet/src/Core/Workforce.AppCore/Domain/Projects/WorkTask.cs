namespace Workforce.AppCore.Domain.Projects;

public sealed class WorkTask
{
    public int Id { get; set; }
    public int ProjectId { get; set; }
    public int? AssignedEmployeeId { get; set; }
    public string Title { get; set; } = string.Empty;
    public string? Description { get; set; }
    public TaskStatus Status { get; set; }
    public TaskPriority Priority { get; set; }
    public DateOnly DueDate { get; set; }
}
