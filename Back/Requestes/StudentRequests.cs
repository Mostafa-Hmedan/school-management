using Back.Entities;

namespace Back.Requestes;

public class CreateStudentRequest
{
    
    public string FirstName { set; get; }
    public string LastName { set; get; }
    public string City { set; get; }
    public string Phone { set; get; }
    public DateOnly BirthDay { set; get; }
    public int ClassId { set; get; }

    public IFormFile? Image { set; get; }
    public string Email { set; get; }
    public string Password { set; get; }
}

public class UpdateStudentRequest
{
    public string FirstName { set; get; }
    public string LastName { set; get; }
    public string City { set; get; }
    public string Phone { set; get; }
    public DateOnly BirthDay { set; get; }
    public int ClassId { set; get; }

    public IFormFile? Image { set; get; }
}
