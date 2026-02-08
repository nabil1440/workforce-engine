using Workforce.AppCore.Abstractions;
using Workforce.AppCore.Domain.Projects;

namespace Workforce.AppCore.Domain.Events;

public sealed record TaskCreated(int TaskId, int ProjectId) : IDomainEvent
{
    public DateTimeOffset OccurredAt { get; } = DateTimeOffset.UtcNow;
}

public sealed record TaskAssigned(int TaskId, int? AssignedEmployeeId) : IDomainEvent
{
    public DateTimeOffset OccurredAt { get; } = DateTimeOffset.UtcNow;
}

public sealed record TaskStatusChanged(int TaskId, TaskStatus FromStatus, TaskStatus ToStatus) : IDomainEvent
{
    public DateTimeOffset OccurredAt { get; } = DateTimeOffset.UtcNow;
}
