using Workforce.AppCore.Abstractions;
using Workforce.AppCore.Domain.Projects;

namespace Workforce.AppCore.Domain.Events;

public sealed record ProjectCreated(int ProjectId) : IDomainEvent
{
    public DateTimeOffset OccurredAt { get; } = DateTimeOffset.UtcNow;
}

public sealed record ProjectUpdated(int ProjectId) : IDomainEvent
{
    public DateTimeOffset OccurredAt { get; } = DateTimeOffset.UtcNow;
}

public sealed record ProjectStatusChanged(int ProjectId, ProjectStatus FromStatus, ProjectStatus ToStatus) : IDomainEvent
{
    public DateTimeOffset OccurredAt { get; } = DateTimeOffset.UtcNow;
}
