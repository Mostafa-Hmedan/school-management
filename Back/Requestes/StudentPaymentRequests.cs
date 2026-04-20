namespace Back.Requestes;

public class CreateStudentPaymentRequest
{
    public int StudentId { get; set; }
    public decimal TotalAmount { get; set; }
    public decimal PaidAmount { get; set; }
    public DateTime PaymentDate { get; set; }
}

public class UpdateStudentPaymentRequest
{
    public decimal? TotalAmount { get; set; }
    public decimal? PaidAmount { get; set; }
    public DateTime? PaymentDate { get; set; }
}
