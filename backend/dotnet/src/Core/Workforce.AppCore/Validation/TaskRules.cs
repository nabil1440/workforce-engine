using Workforce.AppCore.Abstractions.Results;
using ProjectTaskStatus = Workforce.AppCore.Domain.Projects.TaskStatus;
using Workforce.AppCore.Domain.Projects;

namespace Workforce.AppCore.Validation;

public static class TaskRules
{
    public static Result Validate(WorkTask task)
    {
        var result = Guard.NotNull(task, nameof(task));
        if (!result.IsSuccess)
        {
            return result;
        }

        result = Guard.Positive(task.ProjectId, nameof(task.ProjectId));
        if (!result.IsSuccess)
        {
            return result;
        }

        result = Guard.NotEmpty(task.Title, nameof(task.Title));
        if (!result.IsSuccess)
        {
            return result;
        }

        if (task.DueDate == default)
        {
            return Result.Fail(Errors.Invalid(nameof(task.DueDate), "is required"));
        }

        return Result.Success();
    }

    public static Result ValidateStatusChange(ProjectTaskStatus fromStatus, ProjectTaskStatus toStatus)
    {
        return fromStatus == toStatus
            ? Result.Fail(Errors.InvalidState("task status is already set to the requested value"))
            : Result.Success();
    }
}
