namespace Back.Requestes;

public class CreateGradeRequest
{
    public decimal Score { get; set; }
    public string GradeType { get; set; } = string.Empty; // Midterm, Final, etc.
    public int StudentId { get; set; }
    public int TeacherId { get; set; }
    public int SubjectId { get; set; }
}

public class UpdateGradeRequest
{
    public decimal? Score { get; set; }
    public string? GradeType { get; set; }
}
