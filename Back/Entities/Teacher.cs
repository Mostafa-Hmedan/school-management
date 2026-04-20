namespace Back.Entities;

public class Teacher
{
    public int Id { set; get; }
    public string? FirstName { set; get; }
    public string? LastName { set; get; }
    public string? City { set; get; }
    public string? Phone { set; get; }
    public string? ImagePath { set; get; }

     
    public string AppUserId { set; get; }
    public virtual AppUser AppUser { set; get; }

    

    public int SubjectId { set; get; }
    public virtual Subject Subject { set; get; }

    
    public int ClassId { set; get; }
    public virtual Class Class { set; get; }

     
    public virtual List<StudentTeacher> StudentTeachers { set; get; } = [];

     
    public virtual List<Attendance> Attendances { set; get; } = [];

     
    public virtual List<StudentGrade> Grades { set; get; } = [];
    public virtual List<TeacherPayment> TeacherPayments { set; get; } = [];

    // Timetable relations
    public virtual List<TeacherAvailability> TeacherAvailabilities { set; get; } = [];
    public virtual List<ClassSchedule> ClassSchedules { set; get; } = [];
}