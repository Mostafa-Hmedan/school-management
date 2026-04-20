using Back.Requestes;
using Back.Responses;
using Utilities;

namespace Back.Interfaces;

public interface IClassScheduleServices
{
    Task<PagedResponse<ClassScheduleResponse>> GetSchedulesByClass(int classId, int pageNumber, int pageSize);
    Task<PagedResponse<ClassScheduleResponse>> GetSchedulesByTeacher(int teacherId, int pageNumber, int pageSize);
    Task<ClassScheduleResponse> GetSchedule(int id);
    Task<ClassScheduleResponse> CreateSchedule(CreateClassScheduleRequest request);
    Task<ClassScheduleResponse> UpdateSchedule(int id, UpdateClassScheduleRequest request);
    Task<bool> DeleteSchedule(int id);
}
