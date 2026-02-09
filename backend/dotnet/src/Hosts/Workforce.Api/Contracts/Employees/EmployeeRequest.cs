namespace Workforce.Api.Contracts.Employees;

public sealed class EmployeeRequest
{
    public string FirstName { get; init; } = string.Empty;
    public string LastName { get; init; } = string.Empty;
    public string Email { get; init; } = string.Empty;
    public int DepartmentId { get; init; }
    public int DesignationId { get; init; }
    public decimal Salary { get; init; }
    public DateOnly JoiningDate { get; init; }
    public string? Phone { get; init; }
    public string? Address { get; init; }
    public string? City { get; init; }
    public string? Country { get; init; }
    public bool IsActive { get; init; } = true;
}
