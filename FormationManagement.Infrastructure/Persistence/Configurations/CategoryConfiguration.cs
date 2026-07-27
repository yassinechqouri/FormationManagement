using FormationManagement.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace FormationManagement.Infrastructure.Persistence.Configurations;

public class CategoryConfiguration : IEntityTypeConfiguration<Category>
{
    public void Configure(EntityTypeBuilder<Category> builder)
    {
        builder.ToTable("Categories");

        builder.Property(c => c.Name)
            .IsRequired()
            .HasMaxLength(150);

        builder.Property(c => c.Description)
            .HasMaxLength(1000);

        builder.HasIndex(c => c.Name).IsUnique();
    }
}
