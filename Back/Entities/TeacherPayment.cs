namespace Back.Entities;

public class TeacherPayment
{
    public int Id { get; set; }
    public decimal TotalAmount { get; set; }
    public decimal PaidAmount { get; set; }
    public decimal RemainingAmount { get; set; }
    public DateTime PaymentDate { get; set; }

    public int TeacherId { get; set; }
    public Teacher? Teacher { get; set; }
}
