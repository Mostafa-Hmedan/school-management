using Back.Entities;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using System.Reflection;

namespace Back.Data;

// IdentityDbContext هذا السطر وظيفته وراثة الخواص من 
//  و امر هاااام 
//  انشاء الجداول التي تخص ال   AppUser 

public class AppDbContext : IdentityDbContext<AppUser>
{
    public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) { }

    public DbSet<Student> Students { get; set; }
    public DbSet<Teacher> Teachers { get; set; }
    public DbSet<Class> Classes { get; set; }
    public DbSet<Subject> Subjects { get; set; }
    public DbSet<StudentTeacher> StudentTeachers { get; set; }
    public DbSet<StudentGrade> StudentGrades { get; set; }
    public DbSet<Attendance> Attendances { get; set; }
    public DbSet<Enrollment> Enrollments { get; set; }
    public DbSet<RefreshToken> RefreshTokens { get; set; }
    public DbSet<StudentPayment> StudentPayments { get; set; }
    public DbSet<TeacherPayment> TeacherPayments { get; set; }
    public DbSet<Employee> Employees { get; set; }
    public DbSet<EmployeePayment> EmployeePayments { get; set; }
    public DbSet<TeacherAvailability> TeacherAvailabilities { get; set; }
    public DbSet<ClassSchedule> ClassSchedules { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);
        modelBuilder.ApplyConfigurationsFromAssembly(Assembly.GetExecutingAssembly());
    }
}
