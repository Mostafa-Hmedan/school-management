using Back.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Back.Data.Config;

public class StudentGradeConfiguration : IEntityTypeConfiguration<StudentGrade>
{
    public void Configure(EntityTypeBuilder<StudentGrade> builder)
    {
        builder.HasKey(sg => sg.Id);

        builder.Property(sg => sg.Score).HasPrecision(5, 2);
        builder.Property(sg => sg.GradeType).HasMaxLength(50).IsRequired();

        builder.HasOne(sg => sg.Student)
               .WithMany()
               .HasForeignKey(sg => sg.StudentId)
               .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(sg => sg.Teacher)
               .WithMany(t => t.Grades)
               .HasForeignKey(sg => sg.TeacherId)
               .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(sg => sg.Subject)
               .WithMany()
               .HasForeignKey(sg => sg.SubjectId)
               .OnDelete(DeleteBehavior.Restrict);
    }
}
