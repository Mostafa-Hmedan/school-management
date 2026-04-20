namespace Back.Entities;

public class Subject
{
    public int Id { set; get; }
    public string? SubjectName { set; get; }

    public List<Teacher> TeacherName { set; get; } = [];

    // Timetable relation
    public virtual List<ClassSchedule> ClassSchedules { set; get; } = [];
}