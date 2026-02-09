using Workforce.AppCore.Abstractions.Queries;
using Workforce.AppCore.Domain.Projects;

namespace Workforce.Api.Contracts.Queries;

public sealed class ProjectListQuery
{
    public int Page { get; init; } = 1;
    public int PageSize { get; init; } = 20;
    public ProjectStatus? Status { get; init; }
    public string? Search { get; init; }
    public string? Sort { get; init; }
    public SortDirection SortDirection { get; init; } = SortDirection.Asc;
}
