using Back.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Back.Data.Config;

public class StudentTeacherConfiguration : IEntityTypeConfiguration<StudentTeacher>
{
    public void Configure(EntityTypeBuilder<StudentTeacher> builder)
    {
        builder.HasKey(st => st.Id);

        builder.HasOne(st => st.Student)
               .WithMany(s => s.StudentTeachers)
               .HasForeignKey(st => st.StudentId)
               .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(st => st.Teacher)
               .WithMany(t => t.StudentTeachers)
               .HasForeignKey(st => st.TeacherId)
               .OnDelete(DeleteBehavior.Restrict);
    }
}
