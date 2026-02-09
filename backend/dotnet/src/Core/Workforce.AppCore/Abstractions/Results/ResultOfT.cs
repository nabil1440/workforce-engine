namespace Workforce.AppCore.Abstractions.Results;

public sealed class Result<T>
{
    private Result(bool isSuccess, T? value, Error error)
    {
        IsSuccess = isSuccess;
        Value = value;
        Error = error;
    }

    public bool IsSuccess { get; }
    public T? Value { get; }
    public Error Error { get; }

    public static Result<T> Success(T value) => new(true, value, Error.None);
    public static Result<T> Fail(Error error) => new(false, default, error);

    public Result ToResult() => IsSuccess ? Result.Success() : Result.Fail(Error);
}
