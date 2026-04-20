using Back.Requestes;
using Back.Responses;

namespace Back.Interfaces;

public interface IEmployeePaymentServices
{
    Task<IEnumerable<EmployeePaymentResponse>> GetAllEmployeePaymentsAsync(int PageSize, int PageNumber);
    Task<EmployeePaymentResponse?> GetEmployeePaymentByIdAsync(int PaymentId);
    Task<IEnumerable<EmployeePaymentResponse>> GetPaymentsByEmployeeIdAsync(int EmployeeId);
    Task<EmployeePaymentResponse?> CreateEmployeePaymentAsync(CreateEmployeePaymentRequest request);
    Task<EmployeePaymentResponse?> UpdateEmployeePaymentAsync(int PaymentId, UpdateEmployeePaymentRequest request);
    Task<bool> DeleteEmployeePaymentAsync(int PaymentId);
    Task<int> EmployeePaymentCountAsync();
}
