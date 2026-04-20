using Back.Entities;

namespace Back.Responses;

public class TeacherResponse
{
    public int Id { get; set; }
    public string? FirstName { get; set; }
    public string? LastName { get; set; }
    public string? City { get; set; }
    public string? Phone { get; set; }
    public string? ImagePath { get; set; }
    public string SubjectName { get; set; } = string.Empty;
    public string ClassName { get; set; } = string.Empty;

}

public class TeacherSummaryResponse
{
    public int Id { get; set; }
    public string? FirstName { get; set; }
    public string? LastName { get; set; }
    public string SubjectName { get; set; } = string.Empty;
}

public static class TeacherResponseExtensions
{
    public static TeacherResponse ToDto(this Teacher teacher)
    {
        return new TeacherResponse
        {
            Id = teacher.Id,
            FirstName = teacher.FirstName,
            LastName = teacher.LastName,
            City = teacher.City,
            Phone = teacher.Phone,
            ImagePath = teacher.ImagePath,
            SubjectName = teacher.Subject?.SubjectName ?? string.Empty,
            ClassName = teacher.Class?.ClassNumber ?? string.Empty
        };
    }

    public static TeacherSummaryResponse ToSummaryDto(this Teacher teacher)
    {
        return new TeacherSummaryResponse
        {
            Id = teacher.Id,
            FirstName = teacher.FirstName,
            LastName = teacher.LastName,
            SubjectName = teacher.Subject?.SubjectName ?? string.Empty
        };
    }
}
