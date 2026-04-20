using Back.Entities;

namespace Back.Responses;

public class TeacherPaymentResponse
{
    public int Id { get; set; }
    public int TeacherId { get; set; }
    public string TeacherName { get; set; } = string.Empty;
    public decimal TotalAmount { get; set; }
    public decimal PaidAmount { get; set; }
    public decimal RemainingAmount { get; set; }
    public DateTime PaymentDate { get; set; }
}

public static class TeacherPaymentResponseExtensions
{
    public static TeacherPaymentResponse ToDto(this TeacherPayment payment)
    {
        return new TeacherPaymentResponse
        {
            Id = payment.Id,
            TeacherId = payment.TeacherId,
            TeacherName = $"{payment.Teacher?.FirstName} {payment.Teacher?.LastName}",
            TotalAmount = payment.TotalAmount,
            PaidAmount = payment.PaidAmount,
            RemainingAmount = payment.RemainingAmount,
            PaymentDate = payment.PaymentDate
        };
    }
}
