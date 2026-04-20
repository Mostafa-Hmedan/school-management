namespace Back.Requestes;

public class CreateTeacherPaymentRequest
{
    public int TeacherId { get; set; }
    public decimal TotalAmount { get; set; }
    public decimal PaidAmount { get; set; }
    public DateTime PaymentDate { get; set; }
}

public class UpdateTeacherPaymentRequest
{
    public decimal? TotalAmount { get; set; }
    public decimal? PaidAmount { get; set; }
    public DateTime? PaymentDate { get; set; }
}
