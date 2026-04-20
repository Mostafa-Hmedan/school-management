using Back.Entities;

namespace Back.Responses;

public class EmployeeResponse
{
    public int Id { get; set; }
    public string? FirstName { get; set; }
    public string? LastName { get; set; }
    public string? Phone { get; set; }
    public string? City { get; set; }
    public string? JobTitle { get; set; }
    public decimal? Salary { get; set; }
    public string? ImagePath { get; set; }
}

public static class EmployeeResponseExtensions
{
    public static EmployeeResponse ToDto(this Employee employee)
    {
        return new EmployeeResponse
        {
            Id = employee.Id,
            FirstName = employee.FirstName,
            LastName = employee.LastName,
            Phone = employee.Phone,
            City = employee.City,
            JobTitle = employee.JobTitle,
            Salary = employee.Salary,
            ImagePath = employee.ImagePath
        };
    }
}
