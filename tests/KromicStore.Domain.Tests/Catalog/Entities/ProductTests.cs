using FluentAssertions;
using KromicStore.Domain.Catalog.Entities;
using KromicStore.Domain.Catalog.Events;

namespace KromicStore.Domain.Tests.Catalog.Entities;

/// <summary>
/// Domain tests for Product aggregate root.
/// Verifies product creation, lifecycle, soft delete, variants, images, attributes, tags, and inventory.
/// </summary>
public sealed class ProductTests
{
    private readonly Guid _tenantId = Guid.NewGuid();
    private readonly Guid _categoryId = Guid.NewGuid();

    #region Creation Tests

    [Fact]
    public void Create_WithRequiredFields_CreatesProductSuccessfully()
    {
        // Act
        var product = Product.Create(
            tenantId: _tenantId,
            categoryId: _categoryId,
            sku: "PROD-001",
            name: "Test Product");

        // Assert
        product.Should().NotBeNull();
        product.Id.Should().NotBeEmpty();
        product.TenantId.Should().Be(_tenantId);
        product.CategoryId.Should().Be(_categoryId);
        product.Sku.Should().Be("PROD-001");
        product.Name.Should().Be("Test Product");
        product.Status.Should().Be(ProductStatus.Draft);
        product.Price.Should().Be(0);
        product.IsFeatured.Should().BeFalse();
        product.TrackInventory.Should().BeTrue();
        product.Taxable.Should().BeTrue();
    }

    [Fact]
    public void Create_WithAllOptionalFields_StoresAllFields()
    {
        // Act
        var product = Product.Create(
            tenantId: _tenantId,
            categoryId: _categoryId,
            sku: "PROD-002",
            name: "Complete Product",
            customSlug: "complete-product",
            shortDescription: "Short desc",
            description: "Full description",
            productType: ProductType.Digital,
            status: ProductStatus.Active,
            price: 99.99m,
            compareAtPrice: 149.99m,
            costPrice: 50.00m,
            weight: 2.5m,
            length: 10m,
            width: 5m,
            height: 3m,
            isFeatured: true,
            trackInventory: false,
            taxable: false);

        // Assert
        product.Slug.Should().Be("complete-product");
        product.ShortDescription.Should().Be("Short desc");
        product.Description.Should().Be("Full description");
        product.ProductType.Should().Be(ProductType.Digital);
        product.Status.Should().Be(ProductStatus.Active);
        product.Price.Should().Be(99.99m);
        product.CompareAtPrice.Should().Be(149.99m);
        product.CostPrice.Should().Be(50.00m);
        product.Weight.Should().Be(2.5m);
        product.Length.Should().Be(10m);
        product.Width.Should().Be(5m);
        product.Height.Should().Be(3m);
        product.IsFeatured.Should().BeTrue();
        product.TrackInventory.Should().BeFalse();
        product.Taxable.Should().BeFalse();
    }

    [Fact]
    public void Create_WithoutCustomSlug_GeneratesSlugFromName()
    {
        // Act
        var product = Product.Create(
            tenantId: _tenantId,
            categoryId: _categoryId,
            sku: "PROD-003",
            name: "Product With Slug Generation");

        // Assert
        product.Slug.Should().NotBeNullOrEmpty();
        product.Slug.Should().Contain("product");
    }

    [Fact]
    public void Create_InitializesInventory_WhenTrackInventoryTrue()
    {
        // Act
        var product = Product.Create(
            tenantId: _tenantId,
            categoryId: _categoryId,
            sku: "PROD-004",
            name: "Test",
            trackInventory: true);

        // Assert
        product.Inventory.Should().NotBeNull();
        product.Inventory!.ProductId.Should().Be(product.Id);
    }

    [Fact]
    public void Create_WithTrackInventoryFalse_CreatesUnlimitedInventory()
    {
        // Act
        var product = Product.Create(
            tenantId: _tenantId,
            categoryId: _categoryId,
            sku: "PROD-005",
            name: "Test",
            trackInventory: false);

        // Assert
        // When trackInventory is false, inventory is still created but with unlimited quantity
        product.Inventory.Should().NotBeNull();
        product.Inventory!.AvailableQuantity.Should().Be(999999);
    }

    [Fact]
    public void Create_RaisesDomainEvent_ProductCreatedEvent()
    {
        // Act
        var product = Product.Create(
            tenantId: _tenantId,
            categoryId: _categoryId,
            sku: "PROD-006",
            name: "Event Product");

        // Assert
        product.DomainEvents.Should().HaveCount(1);
        product.DomainEvents.First().Should().BeOfType<ProductCreatedEvent>();
    }

    [Fact]
    public void Create_EmptyName_ThrowsException()
    {
        // Act & Assert
        var act = () => Product.Create(
            tenantId: _tenantId,
            categoryId: _categoryId,
            sku: "PROD-007",
            name: "");

        act.Should().Throw<ArgumentException>().WithMessage("*Name*");
    }

    [Fact]
    public void Create_NameTooLong_ThrowsException()
    {
        // Arrange
        var longName = new string('A', 201);

        // Act & Assert
        var act = () => Product.Create(
            tenantId: _tenantId,
            categoryId: _categoryId,
            sku: "PROD-008",
            name: longName);

        act.Should().Throw<ArgumentException>().WithMessage("*Name*");
    }

    [Fact]
    public void Create_EmptySku_ThrowsException()
    {
        // Act & Assert
        var act = () => Product.Create(
            tenantId: _tenantId,
            categoryId: _categoryId,
            sku: "",
            name: "Product");

        act.Should().Throw<ArgumentException>().WithMessage("*SKU*");
    }

    [Fact]
    public void Create_NegativePrice_ThrowsException()
    {
        // Act & Assert
        var act = () => Product.Create(
            tenantId: _tenantId,
            categoryId: _categoryId,
            sku: "PROD-009",
            name: "Product",
            price: -10);

        act.Should().Throw<ArgumentException>().WithMessage("*Price*");
    }

    [Fact]
    public void Create_ComparePriceLessThanPrice_ThrowsException()
    {
        // Act & Assert
        var act = () => Product.Create(
            tenantId: _tenantId,
            categoryId: _categoryId,
            sku: "PROD-010",
            name: "Product",
            price: 100,
            compareAtPrice: 50);

        act.Should().Throw<ArgumentException>().WithMessage("*Compare*");
    }

    [Fact]
    public void Create_InvalidDimension_ThrowsException()
    {
        // Act & Assert
        var act = () => Product.Create(
            tenantId: _tenantId,
            categoryId: _categoryId,
            sku: "PROD-011",
            name: "Product",
            weight: -1);

        act.Should().Throw<ArgumentException>().WithMessage("*Weight*");
    }

    #endregion

    #region Status Transition Tests

    [Fact]
    public void Publish_DraftProduct_ChangesStatusToActive()
    {
        // Arrange
        var product = Product.Create(_tenantId, _categoryId, "PROD-012", "Test");
        product.Status.Should().Be(ProductStatus.Draft);

        // Act
        product.Publish();

        // Assert
        product.Status.Should().Be(ProductStatus.Active);
    }

    [Fact]
    public void Archive_Product_ChangesStatusToArchived()
    {
        // Arrange
        var product = Product.Create(_tenantId, _categoryId, "PROD-013", "Test");

        // Act
        product.Archive();

        // Assert
        product.Status.Should().Be(ProductStatus.Archived);
    }

    #endregion

    #region Update Tests

    [Fact]
    public void Update_WithNewName_ChangesName()
    {
        // Arrange
        var product = Product.Create(_tenantId, _categoryId, "PROD-014", "Original Name");

        // Act
        product.Update(name: "Updated Name");

        // Assert
        product.Name.Should().Be("Updated Name");
    }

    [Fact]
    public void Update_WithNewSku_ChangesSku()
    {
        // Arrange
        var product = Product.Create(_tenantId, _categoryId, "PROD-015", "Test");

        // Act
        product.Update(sku: "NEW-SKU");

        // Assert
        product.Sku.Should().Be("NEW-SKU");
    }

    [Fact]
    public void Update_WithNewPrice_ChangesPrice()
    {
        // Arrange
        var product = Product.Create(_tenantId, _categoryId, "PROD-016", "Test", price: 50);

        // Act
        product.Update(price: 75);

        // Assert
        product.Price.Should().Be(75);
    }

    [Fact]
    public void Update_WithNullFields_DoesNotChangeFields()
    {
        // Arrange
        var product = Product.Create(_tenantId, _categoryId, "PROD-017", "Original Name", price: 50);

        // Act
        product.Update(name: null, price: null);

        // Assert
        product.Name.Should().Be("Original Name");
        product.Price.Should().Be(50);
    }

    [Fact]
    public void Update_RaisesDomainEvent_ProductUpdatedEvent()
    {
        // Arrange
        var product = Product.Create(_tenantId, _categoryId, "PROD-018", "Test");
        product.ClearDomainEvents();

        // Act
        product.Update(name: "Updated");

        // Assert
        product.DomainEvents.Should().HaveCount(1);
        product.DomainEvents.First().Should().BeOfType<ProductUpdatedEvent>();
    }

    #endregion

    #region Soft Delete Tests

    [Fact]
    public void SoftDelete_Product_SetsDeleteFlags()
    {
        // Arrange
        var product = Product.Create(_tenantId, _categoryId, "PROD-019", "Test");
        var now = DateTime.UtcNow;

        // Act
        product.SoftDelete(now, "admin-user");

        // Assert
        product.IsDeleted.Should().BeTrue();
        product.DeletedOnUtc.Should().Be(now);
        product.DeletedBy.Should().Be("admin-user");
    }

    [Fact]
    public void Restore_DeletedProduct_ClearsDeleteFlags()
    {
        // Arrange
        var product = Product.Create(_tenantId, _categoryId, "PROD-020", "Test");
        product.SoftDelete(DateTime.UtcNow, "admin");

        // Act
        product.Restore();

        // Assert
        product.IsDeleted.Should().BeFalse();
        product.DeletedOnUtc.Should().BeNull();
        product.DeletedBy.Should().BeNull();
    }

    #endregion

    #region Variant Management Tests

    [Fact]
    public void AddVariant_CreatesVariant()
    {
        // Arrange
        var product = Product.Create(_tenantId, _categoryId, "PROD-021", "Test");

        // Act
        product.AddVariant("M", "Medium");

        // Assert
        product.Variants.Should().HaveCount(1);
        product.Variants.First().Name.Should().Be("Medium");
    }

    [Fact]
    public void AddVariant_GeneratesSkuWithSuffix()
    {
        // Arrange
        var product = Product.Create(_tenantId, _categoryId, "PROD-022", "Test");

        // Act
        product.AddVariant("L", "Large");

        // Assert
        product.Variants.First().Sku.Should().Contain("PROD-022");
        product.Variants.First().Sku.Should().Contain("L");
    }

    [Fact]
    public void RemoveVariant_RemovesFromCollection()
    {
        // Arrange
        var product = Product.Create(_tenantId, _categoryId, "PROD-023", "Test");
        product.AddVariant("S", "Small");
        var variantId = product.Variants.First().Id;

        // Act
        product.RemoveVariant(variantId);

        // Assert
        product.Variants.Should().BeEmpty();
    }

    [Fact]
    public void RemoveVariant_InvalidId_ThrowsException()
    {
        // Arrange
        var product = Product.Create(_tenantId, _categoryId, "PROD-024", "Test");

        // Act & Assert
        var act = () => product.RemoveVariant(Guid.NewGuid());
        act.Should().Throw<InvalidOperationException>();
    }

    #endregion

    #region Image Management Tests

    [Fact]
    public void AddImage_CreatesImage()
    {
        // Arrange
        var product = Product.Create(_tenantId, _categoryId, "PROD-025", "Test");

        // Act
        product.AddImage("https://example.com/image.jpg", "Product image");

        // Assert
        product.Images.Should().HaveCount(1);
        product.Images.First().Url.Should().Be("https://example.com/image.jpg");
    }

    [Fact]
    public void AddImage_SetsPrimary_WhenIsPrimaryTrue()
    {
        // Arrange
        var product = Product.Create(_tenantId, _categoryId, "PROD-026", "Test");

        // Act
        product.AddImage("https://example.com/image.jpg", isPrimary: true);

        // Assert
        product.Images.First().IsPrimary.Should().BeTrue();
    }

    [Fact]
    public void AddImage_DuplicatePrimary_ThrowsException()
    {
        // Arrange
        var product = Product.Create(_tenantId, _categoryId, "PROD-027", "Test");
        product.AddImage("https://example.com/image1.jpg", isPrimary: true);

        // Act & Assert
        var act = () => product.AddImage("https://example.com/image2.jpg", isPrimary: true);
        act.Should().Throw<InvalidOperationException>();
    }

    [Fact]
    public void RemoveImage_RemovesFromCollection()
    {
        // Arrange
        var product = Product.Create(_tenantId, _categoryId, "PROD-028", "Test");
        product.AddImage("https://example.com/image.jpg");
        var imageId = product.Images.First().Id;

        // Act
        product.RemoveImage(imageId);

        // Assert
        product.Images.Should().BeEmpty();
    }

    [Fact]
    public void SetPrimaryImage_ChangesPrimary()
    {
        // Arrange
        var product = Product.Create(_tenantId, _categoryId, "PROD-029", "Test");
        product.AddImage("https://example.com/image1.jpg", isPrimary: true);
        product.AddImage("https://example.com/image2.jpg", isPrimary: false);
        var secondImageId = product.Images[1].Id;

        // Act
        product.SetPrimaryImage(secondImageId);

        // Assert
        product.Images[0].IsPrimary.Should().BeFalse();
        product.Images[1].IsPrimary.Should().BeTrue();
    }

    #endregion

    #region Attribute Management Tests

    [Fact]
    public void AddAttribute_CreatesAttribute()
    {
        // Arrange
        var product = Product.Create(_tenantId, _categoryId, "PROD-030", "Test");

        // Act
        product.AddAttribute("Color", "Red");

        // Assert
        product.Attributes.Should().HaveCount(1);
        product.Attributes.First().AttributeName.Should().Be("Color");
    }

    [Fact]
    public void AddAttribute_EmptyValue_ThrowsException()
    {
        // Arrange
        var product = Product.Create(_tenantId, _categoryId, "PROD-031", "Test");

        // Act & Assert
        var act = () => product.AddAttribute("Color", "");
        act.Should().Throw<ArgumentException>();
    }

    #endregion

    #region Tag Management Tests

    [Fact]
    public void AddTag_CreatesTag()
    {
        // Arrange
        var product = Product.Create(_tenantId, _categoryId, "PROD-032", "Test");

        // Act
        product.AddTag("New");

        // Assert
        product.Tags.Should().HaveCount(1);
        // Tag value is normalized to lowercase
        product.Tags.First().Tag.Should().Be("new");
    }

    [Fact]
    public void AddTag_DuplicateTag_ThrowsException()
    {
        // Arrange
        var product = Product.Create(_tenantId, _categoryId, "PROD-033", "Test");
        product.AddTag("Sale");

        // Act & Assert
        // Tags are normalized to lowercase and duplicates are checked
        var act = () => product.AddTag("sale");
        act.Should().Throw<InvalidOperationException>();
    }

    [Fact]
    public void RemoveTag_RemovesTag()
    {
        // Arrange
        var product = Product.Create(_tenantId, _categoryId, "PROD-034", "Test");
        product.AddTag("Clearance");

        // Act
        // Tags are normalized to lowercase when added, so must use lowercase when removing
        product.RemoveTag("clearance");

        // Assert
        product.Tags.Should().BeEmpty();
    }

    #endregion

    #region Duplicate Tests

    [Fact]
    public void Duplicate_RaisesDomainEvent_ProductDuplicatedEvent()
    {
        // Arrange
        var product = Product.Create(_tenantId, _categoryId, "PROD-035", "Original");
        product.ClearDomainEvents();

        // Act
        product.Duplicate("NEW-SKU", "Copy of Original");

        // Assert
        product.DomainEvents.Should().HaveCount(1);
        product.DomainEvents.First().Should().BeOfType<ProductDuplicatedEvent>();
    }

    #endregion

    #region Audit Field Tests

    [Fact]
    public void MarkCreated_SetsAuditFields()
    {
        // Arrange
        var product = Product.Create(_tenantId, _categoryId, "PROD-036", "Test");
        var now = DateTime.UtcNow;

        // Act
        product.MarkCreated(now, "user-123");

        // Assert
        product.CreatedAtUtc.Should().Be(now);
        product.CreatedBy.Should().Be("user-123");
        product.ModifiedAtUtc.Should().Be(now);
        product.ModifiedBy.Should().Be("user-123");
    }

    [Fact]
    public void MarkModified_UpdatesModificationFields()
    {
        // Arrange
        var product = Product.Create(_tenantId, _categoryId, "PROD-037", "Test");
        var createdTime = DateTime.UtcNow;
        product.MarkCreated(createdTime, "user-1");
        
        var modifiedTime = createdTime.AddMinutes(5);

        // Act
        product.MarkModified(modifiedTime, "user-2");

        // Assert
        product.CreatedAtUtc.Should().Be(createdTime);
        product.CreatedBy.Should().Be("user-1");
        product.ModifiedAtUtc.Should().Be(modifiedTime);
        product.ModifiedBy.Should().Be("user-2");
    }

    #endregion
}
