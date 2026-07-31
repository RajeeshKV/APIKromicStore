using KromicStore.Domain.Orders.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace KromicStore.Infrastructure.Persistence.Configurations;

/// <summary>
/// EF Core configuration for OrderTimeline value object.
/// </summary>
public sealed class OrderTimelineConfiguration : IEntityTypeConfiguration<OrderTimeline>
{
    public void Configure(EntityTypeBuilder<OrderTimeline> builder)
    {
        builder.HasKey(ot => ot.Id);

        builder.Property(ot => ot.OrderId)
            .IsRequired();

        builder.Property(ot => ot.EventDescription)
            .IsRequired()
            .HasMaxLength(500);

        builder.Property(ot => ot.Actor)
            .IsRequired()
            .HasMaxLength(256);

        builder.Property(ot => ot.CreatedOnUtc)
            .IsRequired();

        // Indexes
        builder.HasIndex(ot => ot.OrderId)
            .HasDatabaseName("IX_OrderTimeline_OrderId");

        builder.HasIndex(ot => ot.CreatedOnUtc)
            .HasDatabaseName("IX_OrderTimeline_CreatedOnUtc");

        builder.ToTable("OrderTimelines");
    }
}

/// <summary>
/// EF Core configuration for OrderNote value object.
/// </summary>
public sealed class OrderNoteConfiguration : IEntityTypeConfiguration<OrderNote>
{
    public void Configure(EntityTypeBuilder<OrderNote> builder)
    {
        builder.HasKey(on => on.Id);

        builder.Property(on => on.OrderId)
            .IsRequired();

        builder.Property(on => on.Content)
            .IsRequired()
            .HasMaxLength(1000);

        builder.Property(on => on.Author)
            .IsRequired()
            .HasMaxLength(256);

        builder.Property(on => on.CreatedOnUtc)
            .IsRequired();

        builder.Property(on => on.IsPublic)
            .IsRequired();

        // Indexes
        builder.HasIndex(on => on.OrderId)
            .HasDatabaseName("IX_OrderNote_OrderId");

        builder.HasIndex(on => on.CreatedOnUtc)
            .HasDatabaseName("IX_OrderNote_CreatedOnUtc");

        builder.ToTable("OrderNotes");
    }
}
