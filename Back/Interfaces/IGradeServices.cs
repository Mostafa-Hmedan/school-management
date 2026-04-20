using Back.Requestes;
using Back.Responses;

namespace Back.Interfaces;
public interface IGradeServices
{
    Task<IEnumerable<GradeResponse>> GetAllGradesAsync(int PageSize, int PageNumber);
    Task<GradeResponse> GetGradeByIdAsync(int GradeId);
    Task<IEnumerable<GradeResponse>> GetGradesByStudentAsync(int StudentId);
    Task<IEnumerable<GradeResponse>> GetGradesBySubjectAsync(int SubjectId);
    Task<IEnumerable<GradeResponse>> GetGradesByTeacherAsync(int TeacherId);
    Task<GradeResponse> CreateGradeAsync(CreateGradeRequest request);
    Task<GradeResponse> UpdateGradeAsync(int GradeId, UpdateGradeRequest request);
    Task<bool> DeleteGradeAsync(int GradeId);
}
