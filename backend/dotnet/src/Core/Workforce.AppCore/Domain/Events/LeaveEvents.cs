using Workforce.AppCore.Abstractions;

namespace Workforce.AppCore.Domain.Events;

public sealed record LeaveRequested(string LeaveId, string Actor, object? Before, object? After) : IDomainEvent
{
    public DateTimeOffset OccurredAt { get; } = DateTimeOffset.UtcNow;
}

public sealed record LeaveApproved(string LeaveId, string Actor, object? Before, object? After) : IDomainEvent
{
    public DateTimeOffset OccurredAt { get; } = DateTimeOffset.UtcNow;
}

public sealed record LeaveRejected(string LeaveId, string Actor, object? Before, object? After) : IDomainEvent
{
    public DateTimeOffset OccurredAt { get; } = DateTimeOffset.UtcNow;
}

public sealed record LeaveCancelled(string LeaveId, string Actor, object? Before, object? After) : IDomainEvent
{
    public DateTimeOffset OccurredAt { get; } = DateTimeOffset.UtcNow;
}
