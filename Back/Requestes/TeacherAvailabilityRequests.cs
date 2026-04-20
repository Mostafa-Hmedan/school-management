using System.ComponentModel.DataAnnotations;

namespace Back.Requestes;

public class CreateTeacherAvailabilityRequest
{
    [Required]
    public int TeacherId { get; set; }

    [Required]
    public DayOfWeek DayOfWeek { get; set; }

    [Required]
    public TimeSpan StartTime { get; set; }

    [Required]
    public TimeSpan EndTime { get; set; }
}

public class UpdateTeacherAvailabilityRequest
{
    [Required]
    public DayOfWeek DayOfWeek { get; set; }

    [Required]
    public TimeSpan StartTime { get; set; }

    [Required]
    public TimeSpan EndTime { get; set; }
}
