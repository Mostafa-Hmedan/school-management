using Back.Entities;
using Back.Requestes;
using Back.Responses;

namespace Back.Interfaces;

public interface IStudentServices
{
    public Task<IEnumerable<StudentResponse>> GetAllStudentAsync(int PageSize, int PageNumber);
    public Task<StudentResponse> GetStudentByIdAsync(int StudentId);
    public Task<IEnumerable<StudentResponse>> GetStudentByNameAsync(string StudentName);
    public Task<IEnumerable<StudentResponse>> GetStudentByClassAsync(string ClassName);
    public Task<bool> DeleteStudentAsync(int studentId);
    public Task<StudentResponse> UpdateStudentAsync(int StudentId, UpdateStudentRequest updateStudentRequest);
    public Task<StudentResponse> CreateStudentAsync(CreateStudentRequest createStudentRequest);

    public Task<int> StudentCount();
    public Task<StudentResponse> GetStudentByUserIdAsync(string userId);
}