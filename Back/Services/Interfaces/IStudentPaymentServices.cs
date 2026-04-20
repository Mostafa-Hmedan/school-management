using Back.Requestes;
using Back.Responses;

namespace Back.Interfaces;

public interface IStudentPaymentServices
{
    Task<IEnumerable<StudentPaymentResponse>> GetAllStudentPaymentsAsync(int PageSize, int PageNumber);
    Task<StudentPaymentResponse> GetStudentPaymentByIdAsync(int PaymentId);
    Task<IEnumerable<StudentPaymentResponse>> GetPaymentsByStudentIdAsync(int StudentId);
    Task<StudentPaymentResponse> CreateStudentPaymentAsync(CreateStudentPaymentRequest request);
    Task<StudentPaymentResponse> UpdateStudentPaymentAsync(int PaymentId, UpdateStudentPaymentRequest request);
    Task<bool> DeleteStudentPaymentAsync(int PaymentId);
    Task<int> StudentPaymentCountAsync();
}
