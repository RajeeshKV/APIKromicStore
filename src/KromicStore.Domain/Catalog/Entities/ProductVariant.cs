using KromicStore.Domain.Catalog.ValueObjects;
using KromicStore.Domain.Common;
using SkuValueObject = KromicStore.Domain.Catalog.ValueObjects.Sku;

namespace KromicStore.Domain.Catalog.Entities;

/// <summary>
/// ProductVariant entity representing a product variant (e.g., size, color, material).
/// Variants are optional and belong to a product.
/// Variant SKU must be unique within tenant.
/// </summary>
public sealed class ProductVariant : BaseEntity
{
    public Guid ProductId { get; private set; }
    public string Sku { get; private set; } = string.Empty;
    public string Name { get; private set; } = string.Empty;
    public decimal PriceAdjustment { get; private set; }
    public int StockQuantity { get; private set; }
    public bool IsActive { get; private set; }

    // Attributes stored as JSON in database
    private readonly List<ProductVariantAttribute> _attributes = [];
    public IReadOnlyList<ProductVariantAttribute> Attributes => _attributes.AsReadOnly();

    private ProductVariant()
    {
    }

    private ProductVariant(Guid id) : base(id)
    {
    }

    public static ProductVariant Create(
        Guid productId,
        string sku,
        string name,
        decimal priceAdjustment = 0,
        Dictionary<string, string>? attributes = null,
        int stockQuantity = 0)
    {
        if (string.IsNullOrWhiteSpace(sku))
            throw new ArgumentException("SKU cannot be empty", nameof(sku));

        if (string.IsNullOrWhiteSpace(name))
            throw new ArgumentException("Name cannot be empty", nameof(name));

        var skuValueObject = SkuValueObject.Create(sku);

        var variant = new ProductVariant(Guid.NewGuid())
        {
            ProductId = productId,
            Sku = skuValueObject.Value,
            Name = name.Trim(),
            PriceAdjustment = priceAdjustment,
            StockQuantity = stockQuantity >= 0 ? stockQuantity : 0,
            IsActive = true
        };

        if (attributes?.Any() == true)
        {
            foreach (var (attrName, attrValue) in attributes)
            {
                if (!string.IsNullOrWhiteSpace(attrName) && !string.IsNullOrWhiteSpace(attrValue))
                {
                    variant._attributes.Add(new ProductVariantAttribute
                    {
                        Name = attrName.Trim(),
                        Value = attrValue.Trim()
                    });
                }
            }
        }

        return variant;
    }

    public void Update(
        string? name = null,
        decimal? priceAdjustment = null,
        Dictionary<string, string>? attributes = null,
        bool? isActive = null)
    {
        if (name is not null)
            Name = name.Trim();

        if (priceAdjustment.HasValue)
            PriceAdjustment = priceAdjustment.Value;

        if (attributes is not null)
        {
            _attributes.Clear();
            foreach (var (attrName, attrValue) in attributes)
            {
                if (!string.IsNullOrWhiteSpace(attrName) && !string.IsNullOrWhiteSpace(attrValue))
                {
                    _attributes.Add(new ProductVariantAttribute
                    {
                        Name = attrName.Trim(),
                        Value = attrValue.Trim()
                    });
                }
            }
        }

        if (isActive.HasValue)
            IsActive = isActive.Value;
    }

    public void UpdateStock(int newQuantity)
    {
        if (newQuantity < 0)
            throw new ArgumentException("Stock quantity cannot be negative", nameof(newQuantity));

        StockQuantity = newQuantity;
    }

    public Dictionary<string, string> GetAttributesDictionary()
    {
        return _attributes.ToDictionary(a => a.Name, a => a.Value);
    }
}

/// <summary>
/// ProductVariantAttribute represents a key-value pair for variant attributes.
/// </summary>
public sealed class ProductVariantAttribute
{
    public string Name { get; set; } = string.Empty;
    public string Value { get; set; } = string.Empty;
}
