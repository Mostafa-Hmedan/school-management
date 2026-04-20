namespace Back.Entities;

public class StudentTeacher
{
    public int Id { set; get; }
    public int StudentId { set; get; }
    public virtual Student Student { set; get; }
    public int TeacherId { set; get; }
    public virtual Teacher Teacher { set; get; }
    // تاريخ بدء التدريس (إضافي)
    public DateTime StartDate { set; get; } = DateTime.Now;
}
