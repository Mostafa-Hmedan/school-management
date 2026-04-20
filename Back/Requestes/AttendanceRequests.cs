namespace Back.Requestes;

public class CreateAttendanceRequest
{
    public DateOnly Date { get; set; }
    public bool IsPresent { get; set; }
    public string? Notes { get; set; }
    public int StudentId { get; set; }
    public int TeacherId { get; set; }
}

public class UpdateAttendanceRequest
{
    public bool? IsPresent { get; set; }
    public string? Notes { get; set; }
}
