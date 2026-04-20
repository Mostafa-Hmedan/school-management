using Back.Requestes;
using Back.Responses;

namespace Back.Interfaces;

public interface IClassServices
{
    Task<IEnumerable<ClassResponse>> GetAllClassesAsync(int PageSize, int PageNumber);
    Task<ClassResponse> GetClassByIdAsync(int ClassId);
    Task<ClassResponse> GetClassByNumberAsync(string ClassNumber);
    Task<ClassResponse> CreateClassAsync(CreateClassRequest request);
    Task<ClassResponse> UpdateClassAsync(int ClassId, UpdateClassRequest request);
    Task<bool> DeleteClassAsync(int ClassId);
    Task<int> ClassCountAsync();
}
