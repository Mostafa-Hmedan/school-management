using Back.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Back.Data.Config;

public class AttendanceConfiguration : IEntityTypeConfiguration<Attendance>
{
    public void Configure(EntityTypeBuilder<Attendance> builder)
    {
        builder.HasKey(a => a.AttendanceId);

        builder.Property(a => a.Notes).HasMaxLength(500);

        builder.HasOne(a => a.student)
               .WithMany(s => s.Attendances)
               .HasForeignKey(a => a.StudentId)
               .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(a => a.teacher)
               .WithMany(t => t.Attendances)
               .HasForeignKey(a => a.TeacherId)
               .OnDelete(DeleteBehavior.Restrict);
    }
}
