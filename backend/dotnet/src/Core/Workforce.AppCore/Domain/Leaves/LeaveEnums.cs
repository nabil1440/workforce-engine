namespace Workforce.AppCore.Domain.Leaves;

public enum LeaveType
{
    Sick = 0,
    Casual = 1,
    Annual = 2,
    Unpaid = 3
}

public enum LeaveStatus
{
    Pending = 0,
    Approved = 1,
    Rejected = 2,
    Cancelled = 3
}
