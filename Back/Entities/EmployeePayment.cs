namespace Back.Entities;

public class EmployeePayment
{
    public int Id { get; set; }
    public decimal TotalAmount { get; set; }
    public decimal PaidAmount { get; set; }
    public decimal RemainingAmount { get; set; }
    public DateTime PaymentDate { get; set; }

    public int EmployeeId { get; set; }
    public Employee? Employee { get; set; }
}
