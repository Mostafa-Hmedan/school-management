using Back.Entities;

namespace Back.Responses;

public class EnrollmentResponse
{
    public int Id { get; set; }
    public string StudentName { get; set; } = string.Empty;
    public string SubjectName { get; set; } = string.Empty;
    public string ClassName { get; set; } = string.Empty;
    public DateTime EnrollmentDate { get; set; }
}

public static class EnrollmentResponseExtensions
{
    public static EnrollmentResponse ToDto(this Enrollment enrollment)
    {
        return new EnrollmentResponse
        {
            Id = enrollment.Id,
            StudentName = $"{enrollment.Student?.FirstName} {enrollment.Student?.LastName}",
            SubjectName = enrollment.Subject?.SubjectName ?? string.Empty,
            ClassName = enrollment.Class?.ClassNumber ?? string.Empty,
            EnrollmentDate = enrollment.EnrollmentDate
        };
    }
}
