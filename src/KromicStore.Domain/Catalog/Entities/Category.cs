using KromicStore.Domain.Catalog.ValueObjects;
using KromicStore.Domain.Common;
using SlugValueObject = KromicStore.Domain.Catalog.ValueObjects.Slug;

namespace KromicStore.Domain.Catalog.Entities;

/// <summary>
/// Category aggregate root representing a product category in the catalog.
/// Supports hierarchical organization with parent/child relationships.
/// Slugs are unique within a tenant. Soft delete is supported.
/// </summary>
public sealed class Category : TenantEntity, IAuditable, ISoftDeletable
{
    public Guid? ParentCategoryId { get; private set; }
    public string Name { get; private set; } = string.Empty;
    public string Slug { get; private set; } = string.Empty;
    public string? Description { get; private set; }
    public int DisplayOrder { get; private set; }
    public bool IsVisible { get; private set; }
    public CategoryStatus Status { get; private set; }
    public string? ImageUrl { get; private set; }
    public string? MetaTitle { get; private set; }
    public string? MetaDescription { get; private set; }

    // Auditing
    public DateTime CreatedAtUtc { get; private set; }
    public string CreatedBy { get; private set; } = string.Empty;
    public DateTime ModifiedAtUtc { get; private set; }
    public string ModifiedBy { get; private set; } = string.Empty;

    // Soft delete
    public bool IsDeleted { get; private set; }
    public DateTime? DeletedOnUtc { get; private set; }
    public string? DeletedBy { get; private set; }

    private Category()
    {
    }

    private Category(Guid id, Guid tenantId) : base(id, tenantId)
    {
    }

    public static Category Create(
        Guid tenantId,
        string name,
        string? customSlug = null,
        string? description = null,
        Guid? parentCategoryId = null,
        int displayOrder = 0,
        bool isVisible = true,
        CategoryStatus status = CategoryStatus.Active,
        string? imageUrl = null)
    {
        ValidateInputs(name, parentCategoryId);

        var slugValue = SlugValueObject.Create(customSlug, name).Value;

        var category = new Category(Guid.NewGuid(), tenantId)
        {
            Name = name.Trim(),
            Slug = slugValue,
            Description = description?.Trim(),
            ParentCategoryId = parentCategoryId,
            DisplayOrder = displayOrder,
            IsVisible = isVisible,
            Status = status,
            ImageUrl = imageUrl
        };

        return category;
    }

    public void Update(
        string? name = null,
        string? customSlug = null,
        string? description = null,
        Guid? parentCategoryId = null,
        int? displayOrder = null,
        bool? isVisible = null,
        CategoryStatus? status = null,
        string? imageUrl = null)
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

        if (description is not null)
            Description = description.Trim();

        if (parentCategoryId.HasValue)
            ValidateCircularReference(parentCategoryId.Value);

        ParentCategoryId = parentCategoryId ?? ParentCategoryId;
        DisplayOrder = displayOrder ?? DisplayOrder;
        IsVisible = isVisible ?? IsVisible;
        Status = status ?? Status;
        ImageUrl = imageUrl ?? ImageUrl;
    }

    public void Archive()
    {
        Status = CategoryStatus.Archived;
        IsVisible = false;
    }

    public void Unarchive()
    {
        Status = CategoryStatus.Active;
    }

    public void MarkCreated(DateTime utcNow, string actor)
    {
        CreatedAtUtc = utcNow;
        CreatedBy = actor;
        ModifiedAtUtc = utcNow;
        ModifiedBy = actor;
    }

    public void MarkModified(DateTime utcNow, string actor)
    {
        ModifiedAtUtc = utcNow;
        ModifiedBy = actor;
    }

    public void SoftDelete(DateTime utcNow, string actor)
    {
        IsDeleted = true;
        DeletedOnUtc = utcNow;
        DeletedBy = actor;
    }

    public void Restore()
    {
        IsDeleted = false;
        DeletedOnUtc = null;
        DeletedBy = null;
    }

    private static void ValidateInputs(string name, Guid? parentCategoryId)
    {
        if (string.IsNullOrWhiteSpace(name))
            throw new ArgumentException("Name cannot be empty", nameof(name));

        if (name.Length > 100)
            throw new ArgumentException("Name cannot exceed 100 characters", nameof(name));
    }

    private void ValidateCircularReference(Guid parentCategoryId)
    {
        if (parentCategoryId == Id)
            throw new InvalidOperationException("A category cannot be its own parent");
    }
}

public enum CategoryStatus
{
    Active = 0,
    Archived = 1
}
