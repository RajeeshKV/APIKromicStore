using KromicStore.Domain.Common;

namespace KromicStore.Domain.Catalog.Entities;

/// <summary>
/// ProductAttribute entity storing dynamic key-value pairs for products.
/// Examples: Material, Brand, Warranty, Capacity, etc.
/// Used for filtering, search, and product details display.
/// </summary>
public sealed class ProductAttribute : BaseEntity, ISoftDeletable
{
    public Guid ProductId { get; private set; }
    public string AttributeName { get; private set; } = string.Empty;
    public string AttributeValue { get; private set; } = string.Empty;

    // Soft delete
    public bool IsDeleted { get; private set; }
    public DateTime? DeletedOnUtc { get; private set; }
    public string? DeletedBy { get; private set; }

    private ProductAttribute()
    {
    }

    private ProductAttribute(Guid id) : base(id)
    {
    }

    public static ProductAttribute Create(Guid productId, string name, string value)
    {
        if (string.IsNullOrWhiteSpace(name))
            throw new ArgumentException("Attribute name cannot be empty", nameof(name));

        if (string.IsNullOrWhiteSpace(value))
            throw new ArgumentException("Attribute value cannot be empty", nameof(value));

        var attribute = new ProductAttribute(Guid.NewGuid())
        {
            ProductId = productId,
            AttributeName = name.Trim(),
            AttributeValue = value.Trim()
        };

        return attribute;
    }

    public void Update(string name, string value)
    {
        if (string.IsNullOrWhiteSpace(name))
            throw new ArgumentException("Attribute name cannot be empty", nameof(name));

        if (string.IsNullOrWhiteSpace(value))
            throw new ArgumentException("Attribute value cannot be empty", nameof(value));

        AttributeName = name.Trim();
        AttributeValue = value.Trim();
    }

    public void SoftDelete(DateTime utcNow, string actor)
    {
        IsDeleted = true;
        DeletedOnUtc = utcNow;
        DeletedBy = actor;
    }
}
