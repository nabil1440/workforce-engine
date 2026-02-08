using Workforce.AppCore.Abstractions;
using ProjectTaskStatus = Workforce.AppCore.Domain.Projects.TaskStatus;

namespace Workforce.AppCore.Domain.Events;

public sealed record TaskCreated(int TaskId, int ProjectId) : IDomainEvent
{
    public DateTimeOffset OccurredAt { get; } = DateTimeOffset.UtcNow;
}

public sealed record TaskAssigned(int TaskId, int? AssignedEmployeeId) : IDomainEvent
{
    public DateTimeOffset OccurredAt { get; } = DateTimeOffset.UtcNow;
}

public sealed record TaskStatusChanged(int TaskId, ProjectTaskStatus FromStatus, ProjectTaskStatus ToStatus) : IDomainEvent
{
    public DateTimeOffset OccurredAt { get; } = DateTimeOffset.UtcNow;
}
