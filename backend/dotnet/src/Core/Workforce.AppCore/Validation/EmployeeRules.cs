using Workforce.AppCore.Abstractions.Results;
using Workforce.AppCore.Domain.Employees;

namespace Workforce.AppCore.Validation;

public static class EmployeeRules
{
    public static Result Validate(Employee employee)
    {
        var result = Guard.NotNull(employee, nameof(employee));
        if (!result.IsSuccess)
        {
            return result;
        }

        result = Guard.NotEmpty(employee.FirstName, nameof(employee.FirstName));
        if (!result.IsSuccess)
        {
            return result;
        }

        result = Guard.NotEmpty(employee.LastName, nameof(employee.LastName));
        if (!result.IsSuccess)
        {
            return result;
        }

        result = Guard.NotEmpty(employee.Email, nameof(employee.Email));
        if (!result.IsSuccess)
        {
            return result;
        }

        result = Guard.Positive(employee.DepartmentId, nameof(employee.DepartmentId));
        if (!result.IsSuccess)
        {
            return result;
        }

        result = Guard.Positive(employee.DesignationId, nameof(employee.DesignationId));
        if (!result.IsSuccess)
        {
            return result;
        }

        result = Guard.Positive(employee.Salary, nameof(employee.Salary));
        if (!result.IsSuccess)
        {
            return result;
        }

        if (employee.JoiningDate == default)
        {
            return Result.Fail(Errors.Invalid(nameof(employee.JoiningDate), "is required"));
        }

        return Result.Success();
    }
}
