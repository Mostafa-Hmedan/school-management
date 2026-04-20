namespace Back.Requestes;

public class CreateSubjectRequest
{
    public string SubjectName { get; set; } = string.Empty;
}

public class UpdateSubjectRequest
{
    public string? SubjectName { get; set; }
}
