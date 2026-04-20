using Back.Requestes;
using Back.Responses;
using Utilities;

namespace Back.Interfaces;

public interface ITeacherAvailabilityServices
{
    Task<PagedResponse<TeacherAvailabilityResponse>> GetTeacherAvailabilities(int teacherId, int pageNumber, int pageSize);
    Task<TeacherAvailabilityResponse> GetTeacherAvailability(int id);
    Task<TeacherAvailabilityResponse> CreateTeacherAvailability(CreateTeacherAvailabilityRequest request);
    Task<TeacherAvailabilityResponse> UpdateTeacherAvailability(int id, UpdateTeacherAvailabilityRequest request);
    Task<bool> DeleteTeacherAvailability(int id);
}
