namespace Back.Entities;

public class Employee
{
    public int Id { get; set; }
    public string? FirstName { get; set; }
    public string? LastName { get; set; }
    public string? Phone { get; set; }
    public string? City { get; set; }
    public string? JobTitle { get; set; }
    public decimal? Salary { get; set; }
    public string? ImagePath { get; set; }

    public virtual List<EmployeePayment> EmployeePayments { get; set; } = [];
}
