using KromicStore.Domain.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace KromicStore.Infrastructure.Persistence.Configurations;

/// <summary>
/// EF Core configuration for the User entity.
/// </summary>
public sealed class UserConfiguration : IEntityTypeConfiguration<User>
{
    public void Configure(EntityTypeBuilder<User> builder)
    {
        builder.HasKey(u => u.Id);

        builder.Property(u => u.Email)
            .IsRequired()
            .HasMaxLength(256);

        builder.Property(u => u.PasswordHash)
            .IsRequired();

        builder.Property(u => u.FirstName)
            .IsRequired()
            .HasMaxLength(100);

        builder.Property(u => u.LastName)
            .IsRequired()
            .HasMaxLength(100);

        builder.Property(u => u.PhoneNumber)
            .HasMaxLength(20);

        builder.Property(u => u.TokenVersion)
            .HasDefaultValue(1);

        // Audit fields
        builder.Property(u => u.CreatedOnUtc)
            .IsRequired();

        builder.Property(u => u.CreatedBy)
            .IsRequired()
            .HasMaxLength(256);

        builder.Property(u => u.ModifiedOnUtc);

        builder.Property(u => u.ModifiedBy)
            .HasMaxLength(256);

        builder.Property(u => u.DeletedOnUtc);

        builder.Property(u => u.DeletedBy)
            .HasMaxLength(256);

        builder.Property(u => u.IsDeleted)
            .HasDefaultValue(false);

        // Indexes
        // UX_Users_Email_Tenant — same email allowed across different tenants
        // but not within the same tenant (null TenantId = SuperAdmin, globally unique)
        builder.HasIndex(u => new { u.Email, u.TenantId })
            .IsUnique()
            .HasDatabaseName("UX_Users_Email_Tenant");

        builder.HasIndex(u => u.TenantId)
            .HasDatabaseName("IX_Users_TenantId");

        builder.HasIndex(u => u.IsDeleted)
            .HasDatabaseName("IX_Users_IsDeleted");

        // Relationships
        builder.HasMany(u => u.RefreshTokens)
            .WithOne()
            .HasForeignKey(rt => rt.UserId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasMany(u => u.UserRoles)
            .WithOne()
            .HasForeignKey(ur => ur.UserId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.ToTable("Users", "public");
    }
}
