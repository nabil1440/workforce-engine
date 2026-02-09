using System.Text.Json;

namespace Workforce.Infrastructure.Messaging;

public sealed record EventEnvelope(string EventType, DateTimeOffset OccurredAt, JsonElement Payload);
