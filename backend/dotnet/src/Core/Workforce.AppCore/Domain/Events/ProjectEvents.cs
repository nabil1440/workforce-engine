using Workforce.AppCore.Abstractions;
using Workforce.AppCore.Domain.Projects;

namespace Workforce.AppCore.Domain.Events;

public sealed record ProjectCreated(int ProjectId, string Actor, object? Before, object? After) : IDomainEvent
{
    public DateTimeOffset OccurredAt { get; } = DateTimeOffset.UtcNow;
}

public sealed record ProjectUpdated(int ProjectId, string Actor, object? Before, object? After) : IDomainEvent
{
    public DateTimeOffset OccurredAt { get; } = DateTimeOffset.UtcNow;
}

public sealed record ProjectStatusChanged(int ProjectId, ProjectStatus FromStatus, ProjectStatus ToStatus, string Actor, object? Before, object? After) : IDomainEvent
{
    public DateTimeOffset OccurredAt { get; } = DateTimeOffset.UtcNow;
}
