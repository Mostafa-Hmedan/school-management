using Back.Entities;

namespace Back.Responses;

public class ClassResponse
{
    public int Id { get; set; }
    public string? ClassNumber { get; set; }
    public Grade StudentStep { get; set; }
    public int StudentsCount { get; set; }
    public int TeachersCount { get; set; }

}

public static class ClassResponseExtensions
{
    public static ClassResponse ToDto(this Class cls)
    {
        return new ClassResponse
        {
            Id = cls.Id,
            ClassNumber = cls.ClassNumber,
            StudentStep = cls.StudentStep,
            StudentsCount = cls.Students?.Count ?? 0,
            TeachersCount = cls.teachers?.Count ?? 0
        };
    }
}
