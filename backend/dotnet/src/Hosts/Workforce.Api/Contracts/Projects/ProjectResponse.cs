using Workforce.AppCore.Domain.Projects;

namespace Workforce.Api.Contracts.Projects;

public sealed class ProjectResponse
{
    public int Id { get; init; }
    public string Name { get; init; } = string.Empty;
    public string? Description { get; init; }
    public ProjectStatus Status { get; init; }
    public DateOnly StartDate { get; init; }
    public DateOnly? EndDate { get; init; }
}
