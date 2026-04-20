using Back.Entities;

namespace Back.Responses;

public class StudentResponse
{
    public int Id { get; set; }
    public string? FirstName { get; set; }
    public string? LastName { get; set; }
    public string? City { get; set; }
    public string? Phone { get; set; }
    public DateOnly BirthDay { get; set; }
    public string? ImagePath { get; set; }
    public int ClassId { get; set; }
    public string ClassName { get; set; } = string.Empty;

    public static StudentResponse ToDto(Student student)
    {
        return new StudentResponse
        {
            Id = student.Id,
            FirstName = student.FirstName,
            LastName = student.LastName,
            City = student.City,
            Phone = student.Phone,
            BirthDay = student.BirthDay,
            ImagePath = student.ImagePath,
            ClassId = student.ClassId,
            ClassName = student.Class?.ClassNumber ?? string.Empty
        };
    }


    public static IEnumerable<StudentResponse> ToDto(IEnumerable<Student> students)
    {
        return students.Select(ToDto);
    }
}




