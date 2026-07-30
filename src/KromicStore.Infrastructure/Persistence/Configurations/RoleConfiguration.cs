using KromicStore.Domain.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace KromicStore.Infrastructure.Persistence.Configurations;

/// <summary>
/// EF Core configuration for the Role entity.
/// </summary>
public sealed class RoleConfiguration : IEntityTypeConfiguration<Role>
{
    public void Configure(EntityTypeBuilder<Role> builder)
    {
        builder.HasKey(r => r.Id);

        builder.Property(r => r.Name)
            .IsRequired()
            .HasMaxLength(100);

        // Audit fields
        builder.Property(r => r.CreatedOnUtc)
            .IsRequired();

        builder.Property(r => r.CreatedBy)
            .IsRequired()
            .HasMaxLength(256);

        builder.Property(r => r.ModifiedOnUtc);

        builder.Property(r => r.ModifiedBy)
            .HasMaxLength(256);

        builder.Property(r => r.DeletedOnUtc);

        builder.Property(r => r.IsDeleted)
            .HasDefaultValue(false);

        // Indexes
        builder.HasIndex(r => r.Name)
            .IsUnique();

        builder.HasIndex(r => r.IsDeleted);

        builder.ToTable("Roles", "public");
    }
}
