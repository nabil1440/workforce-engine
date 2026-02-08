using Workforce.AppCore.Abstractions;

namespace Workforce.AppCore.Domain.Events;

public sealed record LeaveRequested(string LeaveId) : IDomainEvent
{
    public DateTimeOffset OccurredAt { get; } = DateTimeOffset.UtcNow;
}

public sealed record LeaveApproved(string LeaveId) : IDomainEvent
{
    public DateTimeOffset OccurredAt { get; } = DateTimeOffset.UtcNow;
}

public sealed record LeaveRejected(string LeaveId) : IDomainEvent
{
    public DateTimeOffset OccurredAt { get; } = DateTimeOffset.UtcNow;
}

public sealed record LeaveCancelled(string LeaveId) : IDomainEvent
{
    public DateTimeOffset OccurredAt { get; } = DateTimeOffset.UtcNow;
}
