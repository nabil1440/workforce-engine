using Workforce.AppCore.Abstractions.Results;
using Workforce.AppCore.Domain.Projects;

namespace Workforce.AppCore.Validation;

public static class ProjectRules
{
    public static Result Validate(Project project)
    {
        var result = Guard.NotNull(project, nameof(project));
        if (!result.IsSuccess)
        {
            return result;
        }

        result = Guard.NotEmpty(project.Name, nameof(project.Name));
        if (!result.IsSuccess)
        {
            return result;
        }

        if (project.StartDate == default)
        {
            return Result.Fail(Errors.Invalid(nameof(project.StartDate), "is required"));
        }

        if (project.EndDate.HasValue)
        {
            result = Guard.ValidDateOrder(project.StartDate, project.EndDate.Value, nameof(project.StartDate), nameof(project.EndDate));
            if (!result.IsSuccess)
            {
                return result;
            }
        }

        return Result.Success();
    }

    public static Result ValidateStatusChange(ProjectStatus fromStatus, ProjectStatus toStatus)
    {
        return fromStatus == toStatus
            ? Result.Fail(Errors.InvalidState("project status is already set to the requested value"))
            : Result.Success();
    }
}
