using Workforce.AppCore.Domain.Projects;

namespace Workforce.AppCore.Abstractions.Queries;

public sealed class ProjectQuery : PagedQuery
{
    public ProjectStatus? Status { get; init; }
    public string? Search { get; init; }
}
