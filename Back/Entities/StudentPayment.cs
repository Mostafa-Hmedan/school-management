namespace Back.Entities;

public class StudentPayment
{
    public int Id { get; set; }
    public decimal TotalAmount { get; set; }
    public decimal PaidAmount { get; set; }
    public decimal RemainingAmount { get; set; }
    public DateTime PaymentDate { get; set; }

    public int StudentId { get; set; }
    public Student? Student { get; set; }
}
