using Microsoft.AspNetCore.Mvc;
using Workforce.AppCore.Abstractions.Results;

namespace Workforce.Api.Contracts.Common;

public static class ApiResult
{
    public static IActionResult ToActionResult(this Result result, ControllerBase controller)
    {
        return result.IsSuccess ? controller.Ok() : MapError(controller, result.Error);
    }

    public static IActionResult ToActionResult<T>(this Result<T> result, ControllerBase controller)
    {
        return result.IsSuccess && result.Value is not null
            ? controller.Ok(result.Value)
            : MapError(controller, result.Error);
    }

    public static IActionResult ToActionResult<T>(this Result<T> result, ControllerBase controller, Func<T, object> map)
    {
        return result.IsSuccess && result.Value is not null
            ? controller.Ok(map(result.Value))
            : MapError(controller, result.Error);
    }

    private static IActionResult MapError(ControllerBase controller, Error error)
    {
        var problem = new ProblemDetails
        {
            Title = "Request failed",
            Detail = error.Message
        };

        problem.Extensions["code"] = error.Code;

        return error.Code switch
        {
            "not_found" => controller.NotFound(problem),
            "conflict" => controller.Conflict(problem),
            "invalid_state" => controller.Conflict(problem),
            "validation" => controller.BadRequest(problem),
            _ => controller.BadRequest(problem)
        };
    }
}
