using KromicStore.Domain.Catalog.Events;
using KromicStore.Domain.Catalog.ValueObjects;
using KromicStore.Domain.Common;
using SkuValueObject = KromicStore.Domain.Catalog.ValueObjects.Sku;
using SlugValueObject = KromicStore.Domain.Catalog.ValueObjects.Slug;

namespace KromicStore.Domain.Catalog.Entities;

/// <summary>
/// Product aggregate root representing a product in the catalog.
/// Manages product details, variants, images, inventory, and lifecycle.
/// SKUs and slugs are unique within a tenant. Soft delete is supported.
/// </summary>
public sealed class Product : TenantEntity, IAuditable, ISoftDeletable
{
    public Guid CategoryId { get; private set; }
    public string Sku { get; private set; } = string.Empty;
    public string Name { get; private set; } = string.Empty;
    public string Slug { get; private set; } = string.Empty;
    public string? ShortDescription { get; private set; }
    public string? Description { get; private set; }
    public ProductType ProductType { get; private set; }
    public ProductStatus Status { get; private set; }
    public decimal Price { get; private set; }
    public decimal? CompareAtPrice { get; private set; }
    public decimal? CostPrice { get; private set; }
    public decimal? Weight { get; private set; } // kg
    public decimal? Length { get; private set; } // cm
    public decimal? Width { get; private set; }  // cm
    public decimal? Height { get; private set; } // cm
    public bool IsFeatured { get; private set; }
    public bool TrackInventory { get; private set; }
    public bool Taxable { get; private set; }

    // Relationships
    private readonly List<ProductImage> _images = [];
    private readonly List<ProductVariant> _variants = [];
    private readonly List<ProductAttribute> _attributes = [];
    private readonly List<ProductTag> _tags = [];
    private ProductInventory? _inventory;

    public IReadOnlyList<ProductImage> Images => _images.AsReadOnly();
    public IReadOnlyList<ProductVariant> Variants => _variants.AsReadOnly();
    public IReadOnlyList<ProductAttribute> Attributes => _attributes.AsReadOnly();
    public IReadOnlyList<ProductTag> Tags => _tags.AsReadOnly();
    public ProductInventory? Inventory => _inventory;

    // Domain events
    private readonly List<IDomainEvent> _domainEvents = [];
    public new IReadOnlyList<IDomainEvent> DomainEvents => _domainEvents.AsReadOnly();

    private Product()
    {
    }

    private Product(Guid id, Guid tenantId) : base(id, tenantId)
    {
    }

    public static Product Create(
        Guid tenantId,
        Guid categoryId,
        string sku,
        string name,
        string? customSlug = null,
        string? shortDescription = null,
        string? description = null,
        ProductType productType = ProductType.Physical,
        ProductStatus status = ProductStatus.Draft,
        decimal price = 0,
        decimal? compareAtPrice = null,
        decimal? costPrice = null,
        decimal? weight = null,
        decimal? length = null,
        decimal? width = null,
        decimal? height = null,
        bool isFeatured = false,
        bool trackInventory = true,
        bool taxable = true)
    {
        ValidateInputs(name, sku, price, compareAtPrice, weight, length, width, height);

        var skuObj = SkuValueObject.Create(sku);
        var slugObj = SlugValueObject.Create(customSlug, name);

        var product = new Product(Guid.NewGuid(), tenantId)
        {
            CategoryId = categoryId,
            Sku = skuObj.Value,
            Name = name.Trim(),
            Slug = slugObj.Value,
            ShortDescription = shortDescription?.Trim(),
            Description = description?.Trim(),
            ProductType = productType,
            Status = status,
            Price = price,
            CompareAtPrice = compareAtPrice,
            CostPrice = costPrice,
            Weight = weight,
            Length = length,
            Width = width,
            Height = height,
            IsFeatured = isFeatured,
            TrackInventory = trackInventory,
            Taxable = taxable
        };

        // Initialize inventory
        product._inventory = ProductInventory.Create(product.Id, trackInventory);

        // Raise domain event
        product.AddDomainEvent(new ProductCreatedEvent(
            product.Id,
            tenantId,
            categoryId,
            product.Sku,
            product.Name));

        return product;
    }

    public void Update(
        Guid? categoryId = null,
        string? sku = null,
        string? name = null,
        string? customSlug = null,
        string? shortDescription = null,
        string? description = null,
        ProductStatus? status = null,
        decimal? price = null,
        decimal? compareAtPrice = null,
        decimal? costPrice = null,
        decimal? weight = null,
        decimal? length = null,
        decimal? width = null,
        decimal? height = null,
        bool? isFeatured = null,
        bool? taxable = null)
    {
        if (name is not null)
        {
            name = name.Trim();
            if (string.IsNullOrEmpty(name))
                throw new ArgumentException("Name cannot be empty", nameof(name));

            Name = name;

            // Regenerate slug from new name if no custom slug provided
            if (customSlug is null)
            {
                var slugValue = SlugValueObject.GenerateFromName(name);
                Slug = slugValue;
            }
        }

        if (customSlug is not null)
        {
            var slugObj = SlugValueObject.Create(customSlug, Name);
            Slug = slugObj.Value;
        }

        if (sku is not null)
        {
            var skuObj = SkuValueObject.Create(sku);
            Sku = skuObj.Value;
        }

        if (price.HasValue)
        {
            if (price < 0)
                throw new ArgumentException("Price cannot be negative", nameof(price));
            Price = price.Value;
        }

        if (compareAtPrice.HasValue)
        {
            if (compareAtPrice < 0)
                throw new ArgumentException("Compare price cannot be negative", nameof(compareAtPrice));
            if (compareAtPrice < Price)
                throw new ArgumentException("Compare price must be greater than or equal to price", nameof(compareAtPrice));
            CompareAtPrice = compareAtPrice;
        }

        if (costPrice.HasValue)
        {
            if (costPrice < 0)
                throw new ArgumentException("Cost price cannot be negative", nameof(costPrice));
            CostPrice = costPrice;
        }

        if (weight.HasValue && weight <= 0)
            throw new ArgumentException("Weight must be greater than 0", nameof(weight));
        Weight = weight ?? Weight;

        if (length.HasValue && length <= 0)
            throw new ArgumentException("Length must be greater than 0", nameof(length));
        Length = length ?? Length;

        if (width.HasValue && width <= 0)
            throw new ArgumentException("Width must be greater than 0", nameof(width));
        Width = width ?? Width;

        if (height.HasValue && height <= 0)
            throw new ArgumentException("Height must be greater than 0", nameof(height));
        Height = height ?? Height;

        CategoryId = categoryId ?? CategoryId;
        ShortDescription = shortDescription?.Trim() ?? ShortDescription;
        Description = description?.Trim() ?? Description;
        Status = status ?? Status;
        IsFeatured = isFeatured ?? IsFeatured;
        Taxable = taxable ?? Taxable;

        AddDomainEvent(new ProductUpdatedEvent(Id, TenantId));
    }

    public void Duplicate(string newSku, string newName, string? newSlug = null)
    {
        var skuObj = SkuValueObject.Create(newSku);
        var slugObj = SlugValueObject.Create(newSlug, newName);

        AddDomainEvent(new ProductDuplicatedEvent(
            Id,
            TenantId,
            Guid.NewGuid(),
            skuObj.Value,
            newName,
            slugObj.Value));
    }

    public void Archive()
    {
        Status = ProductStatus.Archived;
    }

    public void Publish()
    {
        if (Status == ProductStatus.Draft)
            Status = ProductStatus.Active;
    }

    public void AddImage(string url, string? altText = null, bool isPrimary = false)
    {
        if (string.IsNullOrWhiteSpace(url))
            throw new ArgumentException("Image URL cannot be empty", nameof(url));

        if (isPrimary && _images.Any(i => i.IsPrimary))
            throw new InvalidOperationException("Product already has a primary image");

        var image = ProductImage.Create(Id, url, altText, _images.Count, isPrimary);
        _images.Add(image);

        AddDomainEvent(new ImageUploadedEvent(Id, TenantId, image.Id, url));
    }

    public void RemoveImage(Guid imageId)
    {
        var image = _images.FirstOrDefault(i => i.Id == imageId);
        if (image is null)
            throw new InvalidOperationException("Image not found");

        if (image.IsPrimary && _images.Count == 1)
            throw new InvalidOperationException("Cannot remove the last primary image");

        _images.Remove(image);
    }

    public void SetPrimaryImage(Guid imageId)
    {
        var image = _images.FirstOrDefault(i => i.Id == imageId);
        if (image is null)
            throw new InvalidOperationException("Image not found");

        foreach (var img in _images)
            img.SetPrimary(false);

        image.SetPrimary(true);
    }

    public void AddVariant(
        string skuSuffix,
        string name,
        decimal priceAdjustment = 0,
        Dictionary<string, string>? attributes = null)
    {
        var variantSku = $"{Sku}-{skuSuffix}";
        var skuValue = SkuValueObject.Create(variantSku).Value;

        var variant = ProductVariant.Create(
            Id,
            skuValue,
            name,
            priceAdjustment,
            attributes);

        _variants.Add(variant);

        AddDomainEvent(new VariantCreatedEvent(Id, TenantId, variant.Id, variant.Sku));
    }

    public void RemoveVariant(Guid variantId)
    {
        var variant = _variants.FirstOrDefault(v => v.Id == variantId);
        if (variant is null)
            throw new InvalidOperationException("Variant not found");

        _variants.Remove(variant);
    }

    public void AddAttribute(string name, string value)
    {
        if (string.IsNullOrWhiteSpace(name))
            throw new ArgumentException("Attribute name cannot be empty", nameof(name));

        if (string.IsNullOrWhiteSpace(value))
            throw new ArgumentException("Attribute value cannot be empty", nameof(value));

        var attribute = ProductAttribute.Create(Id, name, value);
        _attributes.Add(attribute);
    }

    public void AddTag(string tag)
    {
        if (string.IsNullOrWhiteSpace(tag))
            throw new ArgumentException("Tag cannot be empty", nameof(tag));

        if (_tags.Any(t => t.Tag == tag))
            throw new InvalidOperationException($"Tag '{tag}' already exists");

        var productTag = ProductTag.Create(Id, tag);
        _tags.Add(productTag);
    }

    public void RemoveTag(string tag)
    {
        var productTag = _tags.FirstOrDefault(t => t.Tag == tag);
        if (productTag is null)
            throw new InvalidOperationException($"Tag '{tag}' not found");

        _tags.Remove(productTag);
    }

    private void AddDomainEvent(IDomainEvent domainEvent)
    {
        _domainEvents.Add(domainEvent);
    }

    public new void ClearDomainEvents()
    {
        _domainEvents.Clear();
        base.ClearDomainEvents();
    }

    private static void ValidateInputs(
        string name,
        string sku,
        decimal price,
        decimal? compareAtPrice,
        decimal? weight,
        decimal? length,
        decimal? width,
        decimal? height)
    {
        if (string.IsNullOrWhiteSpace(name))
            throw new ArgumentException("Name cannot be empty", nameof(name));

        if (name.Length > 200)
            throw new ArgumentException("Name cannot exceed 200 characters", nameof(name));

        if (string.IsNullOrWhiteSpace(sku))
            throw new ArgumentException("SKU cannot be empty", nameof(sku));

        if (price < 0)
            throw new ArgumentException("Price cannot be negative", nameof(price));

        if (compareAtPrice.HasValue && compareAtPrice < price)
            throw new ArgumentException("Compare price must be greater than or equal to price", nameof(compareAtPrice));

        if (weight.HasValue && weight <= 0)
            throw new ArgumentException("Weight must be greater than 0", nameof(weight));

        if (length.HasValue && length <= 0)
            throw new ArgumentException("Length must be greater than 0", nameof(length));

        if (width.HasValue && width <= 0)
            throw new ArgumentException("Width must be greater than 0", nameof(width));

        if (height.HasValue && height <= 0)
            throw new ArgumentException("Height must be greater than 0", nameof(height));
    }
}

public enum ProductType
{
    Physical = 0,
    Digital = 1
}

public enum ProductStatus
{
    Draft = 0,
    Active = 1,
    Archived = 2
}
