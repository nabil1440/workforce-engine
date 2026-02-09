namespace Workforce.AppCore.Domain.Projects;

public sealed class Project
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; }
    public ProjectStatus Status { get; set; }
    public DateOnly StartDate { get; set; }
    public DateOnly? EndDate { get; set; }
    public List<ProjectMember> Members { get; set; } = [];
    public List<WorkTask> Tasks { get; set; } = [];
}
