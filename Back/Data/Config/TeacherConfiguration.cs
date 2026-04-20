using Back.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Back.Data.Config;

public class TeacherConfiguration : IEntityTypeConfiguration<Teacher>
{
    public void Configure(EntityTypeBuilder<Teacher> builder)
    {
        builder.HasKey(t => t.Id);

        builder.Property(t => t.FirstName).HasMaxLength(100).IsRequired();
        builder.Property(t => t.LastName).HasMaxLength(100).IsRequired();
        builder.Property(t => t.City).HasMaxLength(100);
        builder.Property(t => t.Phone).HasMaxLength(20);
        builder.Property(t => t.ImagePath).HasMaxLength(500);

        builder.HasOne(t => t.AppUser)
               .WithOne()
               .HasForeignKey<Teacher>(t => t.AppUserId)
               .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(t => t.Subject)
               .WithMany(s => s.TeacherName)
               .HasForeignKey(t => t.SubjectId)
               .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(t => t.Class)
               .WithMany(c => c.teachers)
               .HasForeignKey(t => t.ClassId)
               .OnDelete(DeleteBehavior.Restrict);
    }
}
