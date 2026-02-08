namespace Workforce.AppCore.Domain.Employees;

public sealed class Employee
{
    public int Id { get; set; }
    public string FirstName { get; set; } = string.Empty;
    public string LastName { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public bool IsActive { get; set; }
    public int DepartmentId { get; set; }
    public int DesignationId { get; set; }
    public decimal Salary { get; set; }
    public DateOnly JoiningDate { get; set; }
    public string? Phone { get; set; }
    public string? Address { get; set; }
    public string? City { get; set; }
    public string? Country { get; set; }
}
