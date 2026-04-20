using Back.Requestes;
using Back.Responses;

namespace Back.Interfaces;

public interface ISubjectServices
{
    Task<IEnumerable<SubjectResponse>> GetAllSubjectsAsync(int PageSize, int PageNumber);
    Task<SubjectResponse> GetSubjectByIdAsync(int SubjectId);
    Task<SubjectResponse> GetSubjectByNameAsync(string SubjectName);
    Task<SubjectResponse> CreateSubjectAsync(CreateSubjectRequest request);
    Task<SubjectResponse> UpdateSubjectAsync(int SubjectId, UpdateSubjectRequest request);
    Task<bool> DeleteSubjectAsync(int SubjectId);
    Task<int> SubjectCountAsync();
}
