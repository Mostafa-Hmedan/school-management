using Back.Requestes;
using Back.Responses;

namespace Back.Interfaces;

public interface ITeacherPaymentServices
{
    Task<IEnumerable<TeacherPaymentResponse>> GetAllTeacherPaymentsAsync(int PageSize, int PageNumber);
    Task<TeacherPaymentResponse> GetTeacherPaymentByIdAsync(int PaymentId);
    Task<IEnumerable<TeacherPaymentResponse>> GetPaymentsByTeacherIdAsync(int TeacherId);
    Task<TeacherPaymentResponse> CreateTeacherPaymentAsync(CreateTeacherPaymentRequest request);
    Task<TeacherPaymentResponse> UpdateTeacherPaymentAsync(int PaymentId, UpdateTeacherPaymentRequest request);
    Task<bool> DeleteTeacherPaymentAsync(int PaymentId);
    Task<int> TeacherPaymentCountAsync();
}
