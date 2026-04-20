namespace Back.Entities;

// حضور
public class Attendance
{
    public int AttendanceId { set; get; }
    public DateOnly Date { set; get; }
    public bool IsPresent { set; get; }
    public string? Notes { set; get; }
    public Student student { set; get; }
    public int StudentId { set; get; }
    public Teacher teacher { set; get; }
    public int TeacherId { set; get; }
}