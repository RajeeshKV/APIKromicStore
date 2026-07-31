using KromicStore.Domain.Common;

namespace KromicStore.Domain.Catalog.Entities;

/// <summary>
/// ProductCollection aggregate root representing a logical grouping of products.
/// Collections can be seasonal, promotional, or curated (e.g., "New Arrivals", "Sale", "Summer Collection").
/// Many-to-many relationship with products.
/// </summary>
public sealed class ProductCollection : TenantEntity, IAuditable, ISoftDeletable
{
    public string Name { get; private set; } = string.Empty;
    public string? Description { get; private set; }
    public int DisplayOrder { get; private set; }
    public CollectionStatus Status { get; private set; }

    private readonly List<ProductCollectionMapping> _productMappings = [];
    public IReadOnlyList<ProductCollectionMapping> ProductMappings => _productMappings.AsReadOnly();

    // Soft delete is inherited from AuditableEntity

    private ProductCollection()
    {
    }

    private ProductCollection(Guid id, Guid tenantId) : base(id, tenantId)
    {
    }

    public static ProductCollection Create(
        Guid tenantId,
        string name,
        string? description = null,
        int displayOrder = 0,
        CollectionStatus status = CollectionStatus.Active)
    {
        if (string.IsNullOrWhiteSpace(name))
            throw new ArgumentException("Name cannot be empty", nameof(name));

        if (name.Length > 100)
            throw new ArgumentException("Name cannot exceed 100 characters", nameof(name));

        var collection = new ProductCollection(Guid.NewGuid(), tenantId)
        {
            Name = name.Trim(),
            Description = description?.Trim(),
            DisplayOrder = displayOrder,
            Status = status
        };

        return collection;
    }

    public void Update(
        string? name = null,
        string? description = null,
        int? displayOrder = null,
        CollectionStatus? status = null)
    {
        if (name is not null)
        {
            if (string.IsNullOrWhiteSpace(name))
                throw new ArgumentException("Name cannot be empty", nameof(name));

            if (name.Length > 100)
                throw new ArgumentException("Name cannot exceed 100 characters", nameof(name));

            Name = name.Trim();
        }

        if (description is not null)
            Description = description.Trim();

        if (displayOrder.HasValue && displayOrder >= 0)
            DisplayOrder = displayOrder.Value;

        if (status.HasValue)
            Status = status.Value;
    }

    public void Archive()
    {
        Status = CollectionStatus.Archived;
    }

    public void Unarchive()
    {
        Status = CollectionStatus.Active;
    }

    public void AddProduct(Guid productId)
    {
        if (_productMappings.Any(m => m.ProductId == productId))
            throw new InvalidOperationException("Product already in collection");

        var mapping = ProductCollectionMapping.Create(Id, productId);
        _productMappings.Add(mapping);
    }

    public void RemoveProduct(Guid productId)
    {
        var mapping = _productMappings.FirstOrDefault(m => m.ProductId == productId);
        if (mapping is null)
            throw new InvalidOperationException("Product not in collection");

        _productMappings.Remove(mapping);
    }

    // Auditing methods inherited from AuditableEntity
}

public sealed class ProductCollectionMapping : BaseEntity
{
    public Guid CollectionId { get; private set; }
    public Guid ProductId { get; private set; }
    public int DisplayOrder { get; private set; }

    private ProductCollectionMapping()
    {
    }

    private ProductCollectionMapping(Guid id) : base(id)
    {
    }

    public static ProductCollectionMapping Create(Guid collectionId, Guid productId, int displayOrder = 0)
    {
        return new ProductCollectionMapping(Guid.NewGuid())
        {
            CollectionId = collectionId,
            ProductId = productId,
            DisplayOrder = displayOrder
        };
    }
}

public enum CollectionStatus
{
    Active = 0,
    Archived = 1
}
