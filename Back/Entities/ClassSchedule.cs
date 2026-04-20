namespace Back.Entities;

public class ClassSchedule
{
    public int Id { get; set; }

    public int ClassId { get; set; }
    public virtual Class Class { get; set; }

    public int SubjectId { get; set; }
    public virtual Subject Subject { get; set; }

    public int TeacherId { get; set; }
    public virtual Teacher Teacher { get; set; }

    // Day of week: 0 = Sunday, 1 = Monday, etc.
    public DayOfWeek DayOfWeek { get; set; }

    public TimeSpan StartTime { get; set; }
    public TimeSpan EndTime { get; set; }
}
