using Microsoft.AspNetCore.Identity;

namespace Back.Entities;


public class AppUser : IdentityUser
{
    public string? FirstName { set; get; }
    public string? LastName { set; get; }
};
