using Workforce.AppCore.Abstractions.Results;
using Workforce.AppCore.Domain.Leaves;

namespace Workforce.AppCore.Validation;

public static class LeaveRules
{
    public static Result Validate(LeaveRequest leaveRequest)
    {
        var result = Guard.NotNull(leaveRequest, nameof(leaveRequest));
        if (!result.IsSuccess)
        {
            return result;
        }

        result = Guard.Positive(leaveRequest.EmployeeId, nameof(leaveRequest.EmployeeId));
        if (!result.IsSuccess)
        {
            return result;
        }

        result = Guard.NotEmpty(leaveRequest.EmployeeName, nameof(leaveRequest.EmployeeName));
        if (!result.IsSuccess)
        {
            return result;
        }

        if (leaveRequest.StartDate == default)
        {
            return Result.Fail(Errors.Invalid(nameof(leaveRequest.StartDate), "is required"));
        }

        if (leaveRequest.EndDate == default)
        {
            return Result.Fail(Errors.Invalid(nameof(leaveRequest.EndDate), "is required"));
        }

        result = Guard.ValidDateOrder(leaveRequest.StartDate, leaveRequest.EndDate, nameof(leaveRequest.StartDate), nameof(leaveRequest.EndDate));
        if (!result.IsSuccess)
        {
            return result;
        }

        return Result.Success();
    }

    public static Result ValidateApproval(LeaveStatus currentStatus, LeaveStatus targetStatus)
    {
        if (currentStatus == targetStatus)
        {
            return Result.Fail(Errors.InvalidState("leave status is already set to the requested value"));
        }

        return currentStatus switch
        {
            LeaveStatus.Pending when targetStatus is LeaveStatus.Approved or LeaveStatus.Rejected or LeaveStatus.Cancelled => Result.Success(),
            LeaveStatus.Approved when targetStatus is LeaveStatus.Cancelled => Result.Success(),
            _ => Result.Fail(Errors.InvalidState("leave status transition is not allowed"))
        };
    }
}
