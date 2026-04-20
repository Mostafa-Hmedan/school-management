namespace Back.Entities;

public class Class
{
    public int Id { get; set; }
    public string? ClassNumber { get; set; }
    public Grade StudentStep { get; set; }
    public List<Student> Students { get; set; } = [];
    public List<Teacher> teachers { get; set; } = [];
    
    // Timetable relation
    public virtual List<ClassSchedule> ClassSchedules { get; set; } = [];
}