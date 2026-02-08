using Workforce.AppCore.Abstractions;

namespace Workforce.AppCore.Domain.Events;

public sealed record EmployeeCreated(int EmployeeId) : IDomainEvent
{
    public DateTimeOffset OccurredAt { get; } = DateTimeOffset.UtcNow;
}

public sealed record EmployeeUpdated(int EmployeeId) : IDomainEvent
{
    public DateTimeOffset OccurredAt { get; } = DateTimeOffset.UtcNow;
}

public sealed record EmployeeDeactivated(int EmployeeId) : IDomainEvent
{
    public DateTimeOffset OccurredAt { get; } = DateTimeOffset.UtcNow;
}
