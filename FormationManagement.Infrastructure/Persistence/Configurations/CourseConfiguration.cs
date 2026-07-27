using FormationManagement.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace FormationManagement.Infrastructure.Persistence.Configurations;

public class CourseConfiguration : IEntityTypeConfiguration<Course>
{
    public void Configure(EntityTypeBuilder<Course> builder)
    {
        builder.ToTable("Courses");

        builder.Property(c => c.Title).IsRequired().HasMaxLength(250);
        builder.Property(c => c.Description).IsRequired().HasMaxLength(4000);
        builder.Property(c => c.Thumbnail).HasMaxLength(500);
        builder.Property(c => c.Price).HasColumnType("decimal(10,2)");
        builder.Property(c => c.Level).HasConversion<string>().HasMaxLength(20);

        builder.HasIndex(c => c.Title);
        builder.HasIndex(c => c.Published);

        builder.HasOne(c => c.Category)
            .WithMany(cat => cat.Courses)
            .HasForeignKey(c => c.CategoryId)
            .OnDelete(DeleteBehavior.Restrict); // a Category with courses cannot be hard-deleted

        builder.HasMany(c => c.Modules)
            .WithOne(m => m.Course)
            .HasForeignKey(m => m.CourseId)
            .OnDelete(DeleteBehavior.Cascade); // deleting a course cleans up its modules

        builder.HasMany(c => c.Enrollments)
            .WithOne(e => e.Course)
            .HasForeignKey(e => e.CourseId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}
