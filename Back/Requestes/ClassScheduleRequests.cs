using System.ComponentModel.DataAnnotations;

namespace Back.Requestes;

public class CreateClassScheduleRequest
{
    [Required]
    public int ClassId { get; set; }

    [Required]
    public int SubjectId { get; set; }

    [Required]
    public int TeacherId { get; set; }

    [Required]
    public DayOfWeek DayOfWeek { get; set; }

    [Required]
    public TimeSpan StartTime { get; set; }

    [Required]
    public TimeSpan EndTime { get; set; }
}

public class UpdateClassScheduleRequest
{
    [Required]
    public int ClassId { get; set; }

    [Required]
    public int SubjectId { get; set; }

    [Required]
    public int TeacherId { get; set; }

    [Required]
    public DayOfWeek DayOfWeek { get; set; }

    [Required]
    public TimeSpan StartTime { get; set; }

    [Required]
    public TimeSpan EndTime { get; set; }
}
