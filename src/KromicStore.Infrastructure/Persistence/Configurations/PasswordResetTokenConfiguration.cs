using KromicStore.Domain.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace KromicStore.Infrastructure.Persistence.Configurations;

/// <summary>
/// EF Core configuration for the PasswordResetToken entity.
/// </summary>
public sealed class PasswordResetTokenConfiguration : IEntityTypeConfiguration<PasswordResetToken>
{
    public void Configure(EntityTypeBuilder<PasswordResetToken> builder)
    {
        builder.HasKey(prt => prt.Id);

        builder.Property(prt => prt.UserId)
            .IsRequired();

        builder.Property(prt => prt.TokenHash)
            .IsRequired();

        builder.Property(prt => prt.ExpiresOnUtc)
            .IsRequired();

        builder.Property(prt => prt.ConsumedOnUtc);

        // Indexes
        builder.HasIndex(prt => prt.UserId);
        builder.HasIndex(prt => prt.ExpiresOnUtc);

        builder.ToTable("PasswordResetTokens", "public");
    }
}
