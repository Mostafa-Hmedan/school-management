using Back.Entities;

namespace Back.Responses;

public class EmployeePaymentResponse
{
    public int Id { get; set; }
    public int EmployeeId { get; set; }
    public string EmployeeName { get; set; } = string.Empty;
    public string? JobTitle { get; set; }
    public decimal TotalAmount { get; set; }
    public decimal PaidAmount { get; set; }
    public decimal RemainingAmount { get; set; }
    public DateTime PaymentDate { get; set; }
}

public static class EmployeePaymentResponseExtensions
{
    public static EmployeePaymentResponse ToDto(this EmployeePayment payment)
    {
        return new EmployeePaymentResponse
        {
            Id = payment.Id,
            EmployeeId = payment.EmployeeId,
            EmployeeName = $"{payment.Employee?.FirstName} {payment.Employee?.LastName}",
            JobTitle = payment.Employee?.JobTitle,
            TotalAmount = payment.TotalAmount,
            PaidAmount = payment.PaidAmount,
            RemainingAmount = payment.RemainingAmount,
            PaymentDate = payment.PaymentDate
        };
    }
}
