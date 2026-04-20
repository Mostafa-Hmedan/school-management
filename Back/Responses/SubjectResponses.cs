using Back.Entities;

namespace Back.Responses;

public class SubjectResponse
{
    public int Id { get; set; }
    public string? SubjectName { get; set; }
    public int TeachersCount { get; set; }

}

public static class SubjectResponseExtensions
{
    public static SubjectResponse ToDto(this Subject subject)
    {
        return new SubjectResponse
        {
            Id = subject.Id,
            SubjectName = subject.SubjectName,
            TeachersCount = subject.TeacherName?.Count ?? 0
        };
    }
}
