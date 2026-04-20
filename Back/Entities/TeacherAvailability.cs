namespace Back.Entities;

public class TeacherAvailability
{
    public int Id { get; set; }
    
    public int TeacherId { get; set; }
    public virtual Teacher Teacher { get; set; }

    // Day of week: 0 = Sunday, 1 = Monday, etc.
    public DayOfWeek DayOfWeek { get; set; }

    public TimeSpan StartTime { get; set; }
    public TimeSpan EndTime { get; set; }
}
