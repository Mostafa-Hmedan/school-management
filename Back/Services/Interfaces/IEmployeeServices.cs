using Back.Requestes;
using Back.Responses;

namespace Back.Interfaces;

public interface IEmployeeServices
{
    Task<IEnumerable<EmployeeResponse>> GetAllEmployeesAsync(int PageSize, int PageNumber);
    Task<EmployeeResponse?> GetEmployeeByIdAsync(int EmployeeId);
    Task<EmployeeResponse?> CreateEmployeeAsync(CreateEmployeeRequest request, IFormFile? image);
    Task<EmployeeResponse?> UpdateEmployeeAsync(int EmployeeId, UpdateEmployeeRequest request, IFormFile? image);
    Task<bool> DeleteEmployeeAsync(int EmployeeId);
    Task<int> EmployeeCountAsync();
}
