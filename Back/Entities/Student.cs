namespace Back.Entities;

public class Student
{
    public int Id { set; get; }
    public string? FirstName { set; get; }
    public string? LastName { set; get; }
    public string? City { set; get; }
    public string? Phone { set; get; }
    public DateOnly BirthDay { set; get; }
    public string? ImagePath { set; get; }


    public string AppUserId { set; get; }
    public virtual AppUser AppUser { set; get; }


    public int ClassId { set; get; }
    public virtual Class Class { set; get; }

    // هادا وسيط ما لاوم يكون  عندي ماني تو ماني 
    public virtual List<StudentTeacher> StudentTeachers { set; get; } = [];
    public virtual List<Attendance> Attendances { set; get; } = [];
    public virtual List<StudentPayment> StudentPayments { set; get; } = [];
}