using Back.Entities;

namespace Back.Responses;

public class GradeResponse
{
    public int Id { get; set; }
    public decimal Score { get; set; }
    public string GradeType { get; set; } = string.Empty;
    public DateTime DateGiven { get; set; }
    public string StudentName { get; set; } = string.Empty;
    public string TeacherName { get; set; } = string.Empty;
    public string SubjectName { get; set; } = string.Empty;

}

public static class GradeResponseExtensions
{
    public static GradeResponse ToDto(this StudentGrade grade)
    {
        return new GradeResponse
        {
            Id = grade.Id,
            Score = grade.Score,
            GradeType = grade.GradeType,
            DateGiven = grade.DateGiven,
            StudentName = $"{grade.Student?.FirstName} {grade.Student?.LastName}",
            TeacherName = $"{grade.Teacher?.FirstName} {grade.Teacher?.LastName}",
            SubjectName = grade.Subject?.SubjectName ?? string.Empty
        };
    }
}
