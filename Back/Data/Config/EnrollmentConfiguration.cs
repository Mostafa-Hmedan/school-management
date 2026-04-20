using Back.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Back.Data.Config;

public class EnrollmentConfiguration : IEntityTypeConfiguration<Enrollment>
{
    public void Configure(EntityTypeBuilder<Enrollment> builder)
    {
        builder.HasKey(e => e.Id);

        builder.Property(e => e.EnrollmentDate).IsRequired();

        // منع تسجيل نفس الطالب في نفس المادة مرتين
        builder.HasIndex(e => new { e.StudentId, e.SubjectId }).IsUnique();

        builder.HasOne(e => e.Student)
               .WithMany()
               .HasForeignKey(e => e.StudentId)
               .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(e => e.Subject)
               .WithMany()
               .HasForeignKey(e => e.SubjectId)
               .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(e => e.Class)
               .WithMany()
               .HasForeignKey(e => e.ClassId)
               .OnDelete(DeleteBehavior.Restrict);
    }
}
