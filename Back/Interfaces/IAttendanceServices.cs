using Back.Requestes;
using Back.Responses;

namespace Back.Interfaces;

public interface IAttendanceServices
{
    Task<IEnumerable<AttendanceResponse>> GetAllAttendancesAsync(int PageSize, int PageNumber);
    Task<AttendanceResponse> GetAttendanceByIdAsync(int AttendanceId);
    Task<IEnumerable<AttendanceResponse>> GetAttendanceByStudentAsync(int StudentId);
    Task<IEnumerable<AttendanceResponse>> GetAttendanceByDateAsync(DateOnly Date);
    Task<AttendanceResponse> CreateAttendanceAsync(CreateAttendanceRequest request);
    Task<AttendanceResponse> UpdateAttendanceAsync(int AttendanceId, UpdateAttendanceRequest request);
    Task<bool> DeleteAttendanceAsync(int AttendanceId);
}
