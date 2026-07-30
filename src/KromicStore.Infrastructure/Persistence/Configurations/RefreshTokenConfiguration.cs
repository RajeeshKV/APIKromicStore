using KromicStore.Domain.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace KromicStore.Infrastructure.Persistence.Configurations;

/// <summary>
/// EF Core configuration for the RefreshToken entity.
/// </summary>
public sealed class RefreshTokenConfiguration : IEntityTypeConfiguration<RefreshToken>
{
    public void Configure(EntityTypeBuilder<RefreshToken> builder)
    {
        builder.HasKey(rt => rt.Id);

        builder.Property(rt => rt.UserId)
            .IsRequired();

        builder.Property(rt => rt.TokenHash)
            .IsRequired();

        builder.Property(rt => rt.ExpiresOnUtc)
            .IsRequired();

        builder.Property(rt => rt.CreatedOnUtc)
            .IsRequired();

        builder.Property(rt => rt.RevokedOnUtc);

        builder.Property(rt => rt.DeviceName)
            .HasMaxLength(256);

        builder.Property(rt => rt.IPAddress)
            .HasMaxLength(45);

        // Indexes
        builder.HasIndex(rt => rt.UserId);
        builder.HasIndex(rt => rt.ExpiresOnUtc);
        builder.HasIndex(rt => rt.RevokedOnUtc);

        builder.ToTable("RefreshTokens", "public");
    }
}
