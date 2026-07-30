using FluentAssertions;
using KromicStore.Application.Features.Catalog.Commands.UpdateProduct;
using KromicStore.Application.Features.Catalog.Abstractions;
using NSubstitute;
using Xunit;

namespace KromicStore.Application.Tests.Features.Catalog.Commands.UpdateProduct;

/// <summary>
/// Validator tests for UpdateProductCommand.
/// Verifies all validation rules are properly enforced.
/// </summary>
public sealed class UpdateProductCommandValidatorTests
{
    private readonly IProductRepository _productRepository;
    private readonly UpdateProductCommandValidator _validator;

    public UpdateProductCommandValidatorTests()
    {
        _productRepository = Substitute.For<IProductRepository>();
        _validator = new UpdateProductCommandValidator(_productRepository);
    }

    [Fact]
    public async Task Validate_ProductIdEmpty_IsInvalid()
    {
        // Arrange
        var command = new UpdateProductCommand(
            ProductId: Guid.Empty,
            Name: "Test");

        // Act
        var result = await _validator.ValidateAsync(command);

        // Assert
        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.PropertyName == "ProductId");
    }

    [Fact]
    public async Task Validate_NameEmpty_IsInvalid()
    {
        // Arrange
        var command = new UpdateProductCommand(
            ProductId: Guid.NewGuid(),
            Name: "");

        // Act
        var result = await _validator.ValidateAsync(command);

        // Assert
        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.PropertyName == "Name");
    }

    [Fact]
    public async Task Validate_NameExceeds200Chars_IsInvalid()
    {
        // Arrange
        var longName = new string('A', 201);
        var command = new UpdateProductCommand(
            ProductId: Guid.NewGuid(),
            Name: longName);

        // Act
        var result = await _validator.ValidateAsync(command);

        // Assert
        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.PropertyName == "Name" && e.ErrorMessage.Contains("200"));
    }

    [Fact]
    public async Task Validate_SkuEmpty_IsInvalid()
    {
        // Arrange
        var command = new UpdateProductCommand(
            ProductId: Guid.NewGuid(),
            Sku: "");

        // Act
        var result = await _validator.ValidateAsync(command);

        // Assert
        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.PropertyName == "Sku");
    }

    [Fact]
    public async Task Validate_SkuExceeds50Chars_IsInvalid()
    {
        // Arrange
        var longSku = new string('A', 51);
        var command = new UpdateProductCommand(
            ProductId: Guid.NewGuid(),
            Sku: longSku);

        // Act
        var result = await _validator.ValidateAsync(command);

        // Assert
        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.PropertyName == "Sku" && e.ErrorMessage.Contains("50"));
    }

    [Fact]
    public async Task Validate_SkuInvalidFormat_IsInvalid()
    {
        // Arrange
        var command = new UpdateProductCommand(
            ProductId: Guid.NewGuid(),
            Sku: "invalid-sku-lowercase");

        // Act
        var result = await _validator.ValidateAsync(command);

        // Assert
        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.PropertyName == "Sku");
    }

    [Fact]
    public async Task Validate_ValidSku_IsValid()
    {
        // Arrange
        var command = new UpdateProductCommand(
            ProductId: Guid.NewGuid(),
            Sku: "VALID-SKU.123");

        // Act
        var result = await _validator.ValidateAsync(command);

        // Assert
        result.Errors.Where(e => e.PropertyName == "Sku").Should().BeEmpty();
    }

    [Fact]
    public async Task Validate_ShortDescriptionExceeds255Chars_IsInvalid()
    {
        // Arrange
        var longDesc = new string('A', 256);
        var command = new UpdateProductCommand(
            ProductId: Guid.NewGuid(),
            ShortDescription: longDesc);

        // Act
        var result = await _validator.ValidateAsync(command);

        // Assert
        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.PropertyName == "ShortDescription");
    }

    [Fact]
    public async Task Validate_DescriptionExceeds5000Chars_IsInvalid()
    {
        // Arrange
        var longDesc = new string('A', 5001);
        var command = new UpdateProductCommand(
            ProductId: Guid.NewGuid(),
            Description: longDesc);

        // Act
        var result = await _validator.ValidateAsync(command);

        // Assert
        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.PropertyName == "Description");
    }

    [Fact]
    public async Task Validate_SlugInvalidFormat_IsInvalid()
    {
        // Arrange
        var command = new UpdateProductCommand(
            ProductId: Guid.NewGuid(),
            CustomSlug: "Invalid-Slug-With-Uppercase");

        // Act
        var result = await _validator.ValidateAsync(command);

        // Assert
        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.PropertyName == "CustomSlug");
    }

    [Fact]
    public async Task Validate_SlugExceeds200Chars_IsInvalid()
    {
        // Arrange
        var longSlug = new string('a', 201);
        var command = new UpdateProductCommand(
            ProductId: Guid.NewGuid(),
            CustomSlug: longSlug);

        // Act
        var result = await _validator.ValidateAsync(command);

        // Assert
        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.PropertyName == "CustomSlug");
    }

    [Fact]
    public async Task Validate_ValidSlug_IsValid()
    {
        // Arrange
        var command = new UpdateProductCommand(
            ProductId: Guid.NewGuid(),
            CustomSlug: "valid-slug-123");

        // Act
        var result = await _validator.ValidateAsync(command);

        // Assert
        result.Errors.Where(e => e.PropertyName == "CustomSlug").Should().BeEmpty();
    }

    [Fact]
    public async Task Validate_NegativePrice_IsInvalid()
    {
        // Arrange
        var command = new UpdateProductCommand(
            ProductId: Guid.NewGuid(),
            Price: -10m);

        // Act
        var result = await _validator.ValidateAsync(command);

        // Assert
        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.PropertyName == "Price");
    }

    [Fact]
    public async Task Validate_ZeroPrice_IsValid()
    {
        // Arrange
        var command = new UpdateProductCommand(
            ProductId: Guid.NewGuid(),
            Price: 0m);

        // Act
        var result = await _validator.ValidateAsync(command);

        // Assert
        result.Errors.Where(e => e.PropertyName == "Price").Should().BeEmpty();
    }

    [Fact]
    public async Task Validate_CompareAtPriceLessThanPrice_IsInvalid()
    {
        // Arrange
        var command = new UpdateProductCommand(
            ProductId: Guid.NewGuid(),
            Price: 100m,
            CompareAtPrice: 50m);

        // Act
        var result = await _validator.ValidateAsync(command);

        // Assert
        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.PropertyName == "CompareAtPrice");
    }

    [Fact]
    public async Task Validate_CompareAtPriceGreaterThanPrice_IsValid()
    {
        // Arrange
        var command = new UpdateProductCommand(
            ProductId: Guid.NewGuid(),
            Price: 100m,
            CompareAtPrice: 150m);

        // Act
        var result = await _validator.ValidateAsync(command);

        // Assert
        result.Errors.Where(e => e.PropertyName == "CompareAtPrice").Should().BeEmpty();
    }

    [Fact]
    public async Task Validate_NegativeCostPrice_IsInvalid()
    {
        // Arrange
        var command = new UpdateProductCommand(
            ProductId: Guid.NewGuid(),
            CostPrice: -5m);

        // Act
        var result = await _validator.ValidateAsync(command);

        // Assert
        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.PropertyName == "CostPrice");
    }

    [Fact]
    public async Task Validate_NegativeWeight_IsInvalid()
    {
        // Arrange
        var command = new UpdateProductCommand(
            ProductId: Guid.NewGuid(),
            Weight: -1m);

        // Act
        var result = await _validator.ValidateAsync(command);

        // Assert
        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.PropertyName == "Weight");
    }

    [Fact]
    public async Task Validate_NegativeLength_IsInvalid()
    {
        // Arrange
        var command = new UpdateProductCommand(
            ProductId: Guid.NewGuid(),
            Length: -5m);

        // Act
        var result = await _validator.ValidateAsync(command);

        // Assert
        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.PropertyName == "Length");
    }

    [Fact]
    public async Task Validate_NegativeWidth_IsInvalid()
    {
        // Arrange
        var command = new UpdateProductCommand(
            ProductId: Guid.NewGuid(),
            Width: -2m);

        // Act
        var result = await _validator.ValidateAsync(command);

        // Assert
        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.PropertyName == "Width");
    }

    [Fact]
    public async Task Validate_NegativeHeight_IsInvalid()
    {
        // Arrange
        var command = new UpdateProductCommand(
            ProductId: Guid.NewGuid(),
            Height: -3m);

        // Act
        var result = await _validator.ValidateAsync(command);

        // Assert
        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.PropertyName == "Height");
    }

    [Fact]
    public async Task Validate_ValidDimensions_IsValid()
    {
        // Arrange
        var command = new UpdateProductCommand(
            ProductId: Guid.NewGuid(),
            Weight: 2.5m,
            Length: 10m,
            Width: 5m,
            Height: 15m);

        // Act
        var result = await _validator.ValidateAsync(command);

        // Assert
        result.Errors.Where(e => e.PropertyName.Contains("Weight") || 
            e.PropertyName.Contains("Length") ||
            e.PropertyName.Contains("Width") ||
            e.PropertyName.Contains("Height")).Should().BeEmpty();
    }

    [Fact]
    public async Task Validate_AllNullFields_IsValid()
    {
        // Arrange - All fields null means no update, which should be valid
        var command = new UpdateProductCommand(ProductId: Guid.NewGuid());

        // Act
        var result = await _validator.ValidateAsync(command);

        // Assert
        result.IsValid.Should().BeTrue();
    }

    [Fact]
    public async Task Validate_OnlyProductIdProvided_IsValid()
    {
        // Arrange
        var command = new UpdateProductCommand(ProductId: Guid.NewGuid());

        // Act
        var result = await _validator.ValidateAsync(command);

        // Assert
        result.IsValid.Should().BeTrue();
    }
}
