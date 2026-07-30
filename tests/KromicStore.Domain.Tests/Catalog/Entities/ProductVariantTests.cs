using FluentAssertions;
using KromicStore.Domain.Catalog.Entities;

namespace KromicStore.Domain.Tests.Catalog.Entities;

/// <summary>
/// Domain tests for ProductVariant entity.
/// Verifies variant creation, updates, and attributes.
/// </summary>
public sealed class ProductVariantTests
{
    private readonly Guid _productId = Guid.NewGuid();

    [Fact]
    public void Create_WithRequiredFields_CreatesVariantSuccessfully()
    {
        // Act
        var variant = ProductVariant.Create(
            productId: _productId,
            sku: "PROD-001-M",
            name: "Medium");

        // Assert
        variant.Should().NotBeNull();
        variant.Id.Should().NotBeEmpty();
        variant.ProductId.Should().Be(_productId);
        variant.Sku.Should().Be("PROD-001-M");
        variant.Name.Should().Be("Medium");
        variant.PriceAdjustment.Should().Be(0);
        variant.IsActive.Should().BeTrue();
    }

    [Fact]
    public void Create_WithAllFields_StoresAllFields()
    {
        // Arrange
        var attributes = new Dictionary<string, string> 
        { 
            { "Size", "Large" },
            { "Color", "Blue" }
        };

        // Act
        var variant = ProductVariant.Create(
            productId: _productId,
            sku: "PROD-001-L",
            name: "Large",
            priceAdjustment: 15.50m,
            stockQuantity: 100,
            attributes: attributes);

        // Assert
        variant.PriceAdjustment.Should().Be(15.50m);
        variant.StockQuantity.Should().Be(100);
        variant.Attributes.Should().Contain(a => a.Name == "Size" && a.Value == "Large");
        variant.Attributes.Should().Contain(a => a.Name == "Color" && a.Value == "Blue");
    }

    [Fact]
    public void Update_WithNewName_ChangesName()
    {
        // Arrange
        var variant = ProductVariant.Create(_productId, "SKU-001", "Original");

        // Act
        variant.Update(name: "Updated");

        // Assert
        variant.Name.Should().Be("Updated");
    }

    [Fact]
    public void Update_WithNewPrice_ChangesPriceAdjustment()
    {
        // Arrange
        var variant = ProductVariant.Create(_productId, "SKU-002", "Test", priceAdjustment: 10);

        // Act
        variant.Update(priceAdjustment: 20);

        // Assert
        variant.PriceAdjustment.Should().Be(20);
    }

    [Fact]
    public void Update_WithNewAttributes_ChangesAttributes()
    {
        // Arrange
        var variant = ProductVariant.Create(_productId, "SKU-003", "Test");
        var newAttributes = new Dictionary<string, string> { { "Color", "Red" } };

        // Act
        variant.Update(attributes: newAttributes);

        // Assert
        variant.Attributes.Should().Contain(a => a.Name == "Color" && a.Value == "Red");
    }

    [Fact]
    public void Update_ToggleActive_ChangesActiveState()
    {
        // Arrange
        var variant = ProductVariant.Create(_productId, "SKU-004", "Test");

        // Act
        variant.Update(isActive: false);

        // Assert
        variant.IsActive.Should().BeFalse();
    }
}
