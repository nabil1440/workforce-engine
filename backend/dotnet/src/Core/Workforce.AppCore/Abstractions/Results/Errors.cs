namespace Workforce.AppCore.Abstractions.Results;

public static class Errors
{
    public static Error Validation(string field, string message) => new("validation", $"{field}: {message}");
    public static Error Null(string field) => Validation(field, "cannot be null");
    public static Error Empty(string field) => Validation(field, "cannot be empty");
    public static Error Invalid(string field, string message) => Validation(field, message);
    public static Error NotFound(string entity, object id) => new("not_found", $"{entity} '{id}' was not found.");
    public static Error InvalidState(string message) => new("invalid_state", message);
    public static Error Conflict(string message) => new("conflict", message);
}
