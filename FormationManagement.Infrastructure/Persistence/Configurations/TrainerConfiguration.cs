using FormationManagement.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace FormationManagement.Infrastructure.Persistence.Configurations;

public class TrainerConfiguration : IEntityTypeConfiguration<Trainer>
{
    public void Configure(EntityTypeBuilder<Trainer> builder)
    {
        builder.ToTable("Trainers");

        builder.Property(t => t.FirstName).IsRequired().HasMaxLength(100);
        builder.Property(t => t.LastName).IsRequired().HasMaxLength(100);
        builder.Property(t => t.Email).IsRequired().HasMaxLength(256);
        builder.Property(t => t.Phone).HasMaxLength(30);
        builder.Property(t => t.Biography).HasMaxLength(2000);
        builder.Property(t => t.Photo).HasMaxLength(500);
        builder.Property(t => t.ApplicationUserId).IsRequired().HasMaxLength(450); // matches AspNetUsers.Id length

        builder.HasIndex(t => t.ApplicationUserId).IsUnique();
        builder.HasIndex(t => t.Email).IsUnique();

        // One Trainer profile can own many Courses.
        builder.HasMany(t => t.Courses)
            .WithOne(c => c.Trainer)
            .HasForeignKey(c => c.TrainerId)
            .OnDelete(DeleteBehavior.Restrict); // don't cascade-delete a trainer's courses by accident
    }
}
