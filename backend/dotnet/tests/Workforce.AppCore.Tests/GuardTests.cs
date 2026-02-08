using Workforce.AppCore.Abstractions.Results;
using Workforce.AppCore.Validation;

namespace Workforce.AppCore.Tests;

public class GuardTests
{
    [Fact]
    public void NotEmpty_FailsForWhitespace()
    {
        var result = Guard.NotEmpty(" ", "name");

        Assert.False(result.IsSuccess);
        Assert.Equal(Errors.Empty("name"), result.Error);
    }

    [Fact]
    public void Positive_FailsForZero()
    {
        var result = Guard.Positive(0, "count");

        Assert.False(result.IsSuccess);
        Assert.Equal(Errors.Invalid("count", "must be greater than 0"), result.Error);
    }

    [Fact]
    public void ValidDateOrder_FailsWhenEndBeforeStart()
    {
        var result = Guard.ValidDateOrder(new DateOnly(2024, 1, 10), new DateOnly(2024, 1, 9), "start", "end");

        Assert.False(result.IsSuccess);
        Assert.Equal(Errors.Invalid("end", "must be on or after start"), result.Error);
    }
}
