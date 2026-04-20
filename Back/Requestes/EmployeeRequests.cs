namespace Back.Requestes;

public class CreateEmployeeRequest
{
    public string? FirstName { get; set; }
    public string? LastName { get; set; }
    public string? Phone { get; set; }
    public string? City { get; set; }
    public string? JobTitle { get; set; }
    public decimal? Salary { get; set; }
}

public class UpdateEmployeeRequest
{
    public string? FirstName { get; set; }
    public string? LastName { get; set; }
    public string? Phone { get; set; }
    public string? City { get; set; }
    public string? JobTitle { get; set; }
    public decimal? Salary { get; set; }
}
