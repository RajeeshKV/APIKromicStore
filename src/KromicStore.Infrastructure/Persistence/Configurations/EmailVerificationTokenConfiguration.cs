using KromicStore.Domain.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace KromicStore.Infrastructure.Persistence.Configurations;

/// <summary>
/// EF Core configuration for the EmailVerificationToken entity.
/// </summary>
public sealed class EmailVerificationTokenConfiguration : IEntityTypeConfiguration<EmailVerificationToken>
{
    public void Configure(EntityTypeBuilder<EmailVerificationToken> builder)
    {
        builder.HasKey(evt => evt.Id);

        builder.Property(evt => evt.UserId)
            .IsRequired();

        builder.Property(evt => evt.TokenHash)
            .IsRequired();

        builder.Property(evt => evt.ExpiresOnUtc)
            .IsRequired();

        builder.Property(evt => evt.ConsumedOnUtc);

        // Indexes
        builder.HasIndex(evt => evt.UserId);
        builder.HasIndex(evt => evt.ExpiresOnUtc);

        builder.ToTable("EmailVerificationTokens", "public");
    }
}
