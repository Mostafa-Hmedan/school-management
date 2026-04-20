using Back.Requestes;
using Back.Responses;

namespace Back.Interfaces;

public interface ITeacherServices
{
    public Task<IEnumerable<TeacherResponse>> GetAllTeacherAsync(int PageSize, int PageNumber);
    public Task<TeacherResponse> GetTeacherByIdAsync(int TeacherId);
    public Task<IEnumerable<TeacherResponse>> GetTeacherByNameAsync(string TeacherName);
    public Task<IEnumerable<TeacherResponse>> GetTeacherByClassAsync(string ClassName);
    public Task<IEnumerable<TeacherResponse>> GetTeacherBySubjectAsync(string SubjectName);
    public Task<TeacherResponse> CreateTeacherAsync(CreateTeacherRequest request);
    public Task<TeacherResponse> UpdateTeacherAsync(int TeacherId, UpdateTeacherRequest request);
    public Task<bool> DeleteTeacherAsync(int TeacherId);
    public Task<int> TeacherCountAsync();
    public Task<TeacherResponse> GetTeacherByUserIdAsync(string userId);
}
