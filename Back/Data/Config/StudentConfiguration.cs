using Back.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Back.Data.Config;

public class StudentConfiguration : IEntityTypeConfiguration<Student>
{
    public void Configure(EntityTypeBuilder<Student> builder)
    {
        builder.HasKey(s => s.Id);

        builder.Property(s => s.FirstName).HasMaxLength(100).IsRequired();
        builder.Property(s => s.LastName).HasMaxLength(100).IsRequired();
        builder.Property(s => s.City).HasMaxLength(100);
        builder.Property(s => s.Phone).HasMaxLength(20);
        builder.Property(s => s.ImagePath).HasMaxLength(500);

        builder.HasOne(s => s.AppUser)
               .WithOne()
               .HasForeignKey<Student>(s => s.AppUserId)
               .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(s => s.Class)
               .WithMany(c => c.Students)
               .HasForeignKey(s => s.ClassId)
               .OnDelete(DeleteBehavior.Restrict);
    }
}
