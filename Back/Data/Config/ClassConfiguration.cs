using Back.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Back.Data.Config;

public class ClassConfiguration : IEntityTypeConfiguration<Class>
{
    public void Configure(EntityTypeBuilder<Class> builder)
    {
        builder.HasKey(c => c.Id);

        builder.Property(c => c.ClassNumber).HasMaxLength(50).IsRequired();
        builder.Property(c => c.StudentStep).HasConversion<int>();
    }
}
