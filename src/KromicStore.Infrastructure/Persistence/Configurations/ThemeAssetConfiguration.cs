using KromicStore.Domain.Tenants;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace KromicStore.Infrastructure.Persistence.Configurations;

/// <summary>
/// Entity configuration for ThemeAsset.
/// Defines database schema, relationships, and constraints.
/// </summary>
public sealed class ThemeAssetConfiguration : IEntityTypeConfiguration<ThemeAsset>
{
    public void Configure(EntityTypeBuilder<ThemeAsset> builder)
    {
        builder.ToTable("ThemeAssets");

        builder.HasKey(ta => ta.Id);

        builder.Property(ta => ta.ThemeId)
            .IsRequired();

        builder.Property(ta => ta.FileName)
            .IsRequired()
            .HasMaxLength(255);

        builder.Property(ta => ta.ContentType)
            .IsRequired()
            .HasMaxLength(100);

        builder.Property(ta => ta.StoragePath)
            .IsRequired()
            .HasMaxLength(500);

        builder.Property(ta => ta.Size)
            .IsRequired()
            .HasColumnType("bigint");

        builder.Property(ta => ta.AssetType)
            .IsRequired()
            .HasConversion<int>();

        builder.Property(ta => ta.Description)
            .HasMaxLength(500);

        builder.Property(ta => ta.IsActive)
            .IsRequired()
            .HasDefaultValue(true);

        builder.Property(ta => ta.PublicUrl)
            .HasMaxLength(500);

        builder.Property(ta => ta.CreatedOnUtc)
            .HasColumnName("CreatedAtUtc")
            .IsRequired();

        builder.Property(ta => ta.CreatedBy)
            .IsRequired()
            .HasMaxLength(500);

        builder.Property(ta => ta.ModifiedOnUtc)
            .HasColumnName("ModifiedAtUtc");

        builder.Property(ta => ta.ModifiedBy)
            .HasMaxLength(500);

        builder.Property(ta => ta.DeletedOnUtc)
            .HasColumnName("DeletedAtUtc");

        builder.Property(ta => ta.DeletedBy)
            .HasMaxLength(500);

        builder.Property(ta => ta.IsDeleted)
            .IsRequired()
            .HasDefaultValue(false);

        // Foreign key to Theme
        builder.HasOne<Theme>()
            .WithMany()
            .HasForeignKey(ta => ta.ThemeId)
            .OnDelete(DeleteBehavior.Cascade);

        // Indices
        builder.HasIndex(ta => ta.ThemeId);
        builder.HasIndex(ta => ta.AssetType);
        builder.HasIndex(ta => ta.IsActive);
        builder.HasIndex(ta => new { ta.ThemeId, ta.AssetType });
    }
}
