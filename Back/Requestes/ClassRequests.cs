using Back.Entities;
namespace Back.Requestes;
public class CreateClassRequest
{
    public string ClassNumber { get; set; } = string.Empty;
    public Grade StudentStep { get; set; }
}
public class UpdateClassRequest
{
    public string? ClassNumber { get; set; }
    public Grade? StudentStep { get; set; }
}