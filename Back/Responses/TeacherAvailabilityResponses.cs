using Back.Entities;

namespace Back.Responses;

public record TeacherAvailabilityResponse(
    int Id,
    int TeacherId,
    string TeacherName,
    DayOfWeek DayOfWeek,
    TimeSpan StartTime,
    TimeSpan EndTime
);

public static class TeacherAvailabilityResponseExtensions
{
    public static TeacherAvailabilityResponse ToDto(this TeacherAvailability availability)
    {
        return new TeacherAvailabilityResponse(
            availability.Id,
            availability.TeacherId,
            $"{availability.Teacher?.FirstName} {availability.Teacher?.LastName}".Trim(),
            availability.DayOfWeek,
            availability.StartTime,
            availability.EndTime
        );
    }
}
