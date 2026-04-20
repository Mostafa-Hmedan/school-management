using Back.Entities;

namespace Back.Responses;

public record ClassScheduleResponse(
    int Id,
    int ClassId,
    string ClassName,
    int SubjectId,
    string SubjectName,
    int TeacherId,
    string TeacherName,
    DayOfWeek DayOfWeek,
    TimeSpan StartTime,
    TimeSpan EndTime
);

public static class ClassScheduleResponseExtensions
{
    public static ClassScheduleResponse ToDto(this ClassSchedule schedule)
    {
        return new ClassScheduleResponse(
            schedule.Id,
            schedule.ClassId,
            schedule.Class?.ClassNumber ?? string.Empty,
            schedule.SubjectId,
            schedule.Subject?.SubjectName ?? string.Empty,
            schedule.TeacherId,
            $"{schedule.Teacher?.FirstName} {schedule.Teacher?.LastName}".Trim(),
            schedule.DayOfWeek,
            schedule.StartTime,
            schedule.EndTime
        );
    }
}
