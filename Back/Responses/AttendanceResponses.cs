using Back.Entities;
namespace Back.Responses;
public class AttendanceResponse
{
    public int AttendanceId { get; set; }
    public DateOnly Date { get; set; }
    public bool IsPresent { get; set; }
    public string? Notes { get; set; }
    public string StudentName { get; set; } = string.Empty;
    public string TeacherName { get; set; } = string.Empty;

}
public static class AttendanceResponseExtensions
{
    public static AttendanceResponse ToDto(this Attendance attendance)
    {
        return new AttendanceResponse
        {
            AttendanceId = attendance.AttendanceId,
            Date = attendance.Date,
            IsPresent = attendance.IsPresent,
            Notes = attendance.Notes,
            StudentName = $"{attendance.student?.FirstName} {attendance.student?.LastName}",
            TeacherName = $"{attendance.teacher?.FirstName} {attendance.teacher?.LastName}"
        };
    }
}