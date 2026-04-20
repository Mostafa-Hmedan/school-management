namespace Back.Requestes;

public class CreateTeacherRequest
{
    public string FirstName { get; set; } = string.Empty;
    public string LastName { get; set; } = string.Empty;
    public string? City { get; set; }
    public string? Phone { get; set; }
    public int SubjectId { get; set; }
    public int ClassId { get; set; }
    public IFormFile? Image { get; set; }
    public string Email { get; set; } = string.Empty;
    public string Password { get; set; } = string.Empty;
}

public class UpdateTeacherRequest
{
    public string? FirstName { get; set; }
    public string? LastName { get; set; }
    public string? City { get; set; }
    public string? Phone { get; set; }
    public int? SubjectId { get; set; }
    public int? ClassId { get; set; }
    public IFormFile? Image { get; set; }
}
