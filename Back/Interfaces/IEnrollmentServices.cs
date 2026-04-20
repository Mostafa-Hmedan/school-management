using Back.Requestes;
using Back.Responses;

namespace Back.Interfaces;

public interface IEnrollmentServices
{
    Task<IEnumerable<EnrollmentResponse>> GetAllEnrollmentsAsync(int PageSize, int PageNumber);
    Task<EnrollmentResponse> GetEnrollmentByIdAsync(int EnrollmentId);
    Task<IEnumerable<EnrollmentResponse>> GetEnrollmentsByStudentAsync(int StudentId);
    Task<IEnumerable<EnrollmentResponse>> GetEnrollmentsByClassAsync(int ClassId);
    Task<IEnumerable<EnrollmentResponse>> GetEnrollmentsBySubjectAsync(int SubjectId);
    Task<EnrollmentResponse> CreateEnrollmentAsync(CreateEnrollmentRequest request);
    Task<bool> DeleteEnrollmentAsync(int EnrollmentId);

    Task<int> EnrollmentCountAsync();
}
