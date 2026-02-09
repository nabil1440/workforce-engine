using Workforce.AppCore.Abstractions.Results;

namespace Workforce.AppCore.Validation;

public static class Guard
{
    public static Result NotNull(object? value, string field)
    {
        return value is null ? Result.Fail(Errors.Null(field)) : Result.Success();
    }

    public static Result NotEmpty(string? value, string field)
    {
        return string.IsNullOrWhiteSpace(value) ? Result.Fail(Errors.Empty(field)) : Result.Success();
    }

    public static Result Positive(int value, string field)
    {
        return value <= 0 ? Result.Fail(Errors.Invalid(field, "must be greater than 0")) : Result.Success();
    }

    public static Result Positive(decimal value, string field)
    {
        return value <= 0 ? Result.Fail(Errors.Invalid(field, "must be greater than 0")) : Result.Success();
    }

    public static Result ValidDateOrder(DateOnly start, DateOnly end, string startField, string endField)
    {
        return end < start ? Result.Fail(Errors.Invalid(endField, $"must be on or after {startField}")) : Result.Success();
    }
}
