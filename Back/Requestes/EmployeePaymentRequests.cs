namespace Back.Requestes;

public class CreateEmployeePaymentRequest
{
    public int EmployeeId { get; set; }
    public decimal TotalAmount { get; set; }
    public decimal PaidAmount { get; set; }
    public DateTime PaymentDate { get; set; }
}

public class UpdateEmployeePaymentRequest
{
    public decimal? TotalAmount { get; set; }
    public decimal? PaidAmount { get; set; }
    public DateTime? PaymentDate { get; set; }
}
