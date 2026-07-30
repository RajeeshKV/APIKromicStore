using FluentAssertions;
using KromicStore.Domain.Catalog.Entities;

namespace KromicStore.Domain.Tests.Catalog.Entities;

/// <summary>
/// Domain tests for ProductImage entity.
/// Verifies image creation and primary image management.
/// </summary>
public sealed class ProductImageTests
{
    private readonly Guid _productId = Guid.NewGuid();

    [Fact]
    public void Create_WithRequiredFields_CreatesImageSuccessfully()
    {
        // Act
        var image = ProductImage.Create(
            productId: _productId,
            url: "https://example.com/image.jpg",
            displayOrder: 0);

        // Assert
        image.Should().NotBeNull();
        image.Id.Should().NotBeEmpty();
        image.ProductId.Should().Be(_productId);
        image.Url.Should().Be("https://example.com/image.jpg");
        image.DisplayOrder.Should().Be(0);
        image.IsPrimary.Should().BeFalse();
    }

    [Fact]
    public void Create_WithAllFields_StoresAllFields()
    {
        // Act
        var image = ProductImage.Create(
            productId: _productId,
            url: "https://example.com/image.jpg",
            altText: "Product front view",
            displayOrder: 1,
            isPrimary: true);

        // Assert
        image.AltText.Should().Be("Product front view");
        image.IsPrimary.Should().BeTrue();
    }

    [Fact]
    public void SetPrimary_True_SetsPrimaryFlag()
    {
        // Arrange
        var image = ProductImage.Create(_productId, "https://example.com/image.jpg", displayOrder: 0);

        // Act
        image.SetPrimary(true);

        // Assert
        image.IsPrimary.Should().BeTrue();
    }

    [Fact]
    public void SetPrimary_False_ClearsPrimaryFlag()
    {
        // Arrange
        var image = ProductImage.Create(
            _productId,
            "https://example.com/image.jpg",
            displayOrder: 0,
            isPrimary: true);

        // Act
        image.SetPrimary(false);

        // Assert
        image.IsPrimary.Should().BeFalse();
    }
}
