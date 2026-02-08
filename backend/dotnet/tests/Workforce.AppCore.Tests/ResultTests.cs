using Workforce.AppCore.Abstractions.Results;

namespace Workforce.AppCore.Tests;

public class ResultTests
{
    [Fact]
    public void SuccessResult_HasNoError()
    {
        var result = Result.Success();

        Assert.True(result.IsSuccess);
        Assert.Equal(Error.None, result.Error);
    }

    [Fact]
    public void FailResult_HasError()
    {
        var error = Errors.Invalid("field", "is invalid");
        var result = Result.Fail(error);

        Assert.False(result.IsSuccess);
        Assert.Equal(error, result.Error);
    }

    [Fact]
    public void SuccessResultOfT_CarriesValue()
    {
        var result = Result<string>.Success("ok");

        Assert.True(result.IsSuccess);
        Assert.Equal("ok", result.Value);
        Assert.Equal(Error.None, result.Error);
    }
}
