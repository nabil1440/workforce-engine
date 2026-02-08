namespace Workforce.AppCore.Abstractions;

public interface IDomainEvent
{
    DateTimeOffset OccurredAt { get; }
}
