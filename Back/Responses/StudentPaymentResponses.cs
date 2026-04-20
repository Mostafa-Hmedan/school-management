using Back.Entities;

namespace Back.Responses;

public class StudentPaymentResponse
{
    public int Id { get; set; }
    public int StudentId { get; set; }
    public string StudentName { get; set; } = string.Empty;
    public decimal TotalAmount { get; set; }
    public decimal PaidAmount { get; set; }
    public decimal RemainingAmount { get; set; }
    public DateTime PaymentDate { get; set; }
}

public static class StudentPaymentResponseExtensions
{
    public static StudentPaymentResponse ToDto(this StudentPayment payment)
    {
        return new StudentPaymentResponse
        {
            Id = payment.Id,
            StudentId = payment.StudentId,
            StudentName = $"{payment.Student?.FirstName} {payment.Student?.LastName}",
            TotalAmount = payment.TotalAmount,
            PaidAmount = payment.PaidAmount,
            RemainingAmount = payment.RemainingAmount,
            PaymentDate = payment.PaymentDate
        };
    }
}
