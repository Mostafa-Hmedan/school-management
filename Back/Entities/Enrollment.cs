namespace Back.Entities;

// التسجيل
public class Enrollment
{
    public int Id { set; get; }

    public int StudentId { set; get; }
    public Student Student { set; get; }

    public int SubjectId { set; get; }
    public Subject Subject { set; get; }

    public int ClassId { set; get; }
    public Class Class { set; get; }

    public DateTime EnrollmentDate { set; get; }
}