using FluentAssertions;
using KromicStore.Application.Features.Catalog.Commands.CreateProduct;
using KromicStore.Application.Features.Catalog.Abstractions;
using NSubstitute;

namespace KromicStore.Application.Tests.Features.Catalog.Commands.CreateProduct;

/// <summary>
/// Validator tests for CreateProductCommand.
/// Verifies all validation rules including field presence, format, ranges, and constraints.
/// </summary>
public sealed class CreateProductCommandValidatorTests
{
    private readonly IProductRepository _repository;
    private readonly CreateProductCommandValidator _validator;

    public CreateProductCommandValidatorTests()
    {
        _repository = Substitute.For<IProductRepository>();
        _validator = new CreateProductCommandValidator(_repository);
    }

    #region Required Field Tests

    [Fact]
    public void Validate_CategoryIdEmpty_HasError()
    {
        // Arrange
        var command = new CreateProductCommand(
            CategoryId: Guid.Empty,
            Name: "Valid",
            Sku: "VALID-001");

        // Act
        var result = _validator.Validate(command);

        // Assert
        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.PropertyName == "CategoryId");
    }

    [Fact]
    public void Validate_NameEmpty_HasError()
    {
        // Arrange
        var command = new CreateProductCommand(
            CategoryId: Guid.NewGuid(),
            Name: "",
            Sku: "VALID-002");

        // Act
        var result = _validator.Validate(command);

        // Assert
        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.PropertyName == "Name");
    }

    [Fact]
    public void Validate_SkuEmpty_HasError()
    {
        // Arrange
        var command = new CreateProductCommand(
            CategoryId: Guid.NewGuid(),
            Name: "Valid",
            Sku: "");

        // Act
        var result = _validator.Validate(command);

        // Assert
        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.PropertyName == "Sku");
    }

    #endregion

    #region Length Validation Tests

    [Fact]
    public void Validate_NameExceedsMaxLength_HasError()
    {
        // Arrange
        var longName = new string('A', 201);
        var command = new CreateProductCommand(
            CategoryId: Guid.NewGuid(),
            Name: longName,
            Sku: "VALID-003");

        // Act
        var result = _validator.Validate(command);

        // Assert
        result.IsValid.Should().BeFalse();
    }

    [Fact]
    public void Validate_SkuExceedsMaxLength_HasError()
    {
        // Arrange
        var longSku = new string('A', 51);
        var command = new CreateProductCommand(
            CategoryId: Guid.NewGuid(),
            Name: "Valid",
            Sku: longSku);

        // Act
        var result = _validator.Validate(command);

        // Assert
        result.IsValid.Should().BeFalse();
    }

    [Fact]
    public void Validate_ShortDescriptionExceedsMaxLength_HasError()
    {
        // Arrange
        var longDesc = new string('A', 256);
        var command = new CreateProductCommand(
            CategoryId: Guid.NewGuid(),
            Name: "Valid",
            Sku: "VALID-004",
            ShortDescription: longDesc);

        // Act
        var result = _validator.Validate(command);

        // Assert
        result.IsValid.Should().BeFalse();
    }

    [Fact]
    public void Validate_DescriptionExceedsMaxLength_HasError()
    {
        // Arrange
        var longDesc = new string('A', 5001);
        var command = new CreateProductCommand(
            CategoryId: Guid.NewGuid(),
            Name: "Valid",
            Sku: "VALID-005",
            Description: longDesc);

        // Act
        var result = _validator.Validate(command);

        // Assert
        result.IsValid.Should().BeFalse();
    }

    #endregion

    #region SKU Format Tests

    [Fact]
    public void Validate_SkuWithInvalidFormat_HasError()
    {
        // Arrange
        var command = new CreateProductCommand(
            CategoryId: Guid.NewGuid(),
            Name: "Valid",
            Sku: "invalid-lowercase");

        // Act
        var result = _validator.Validate(command);

        // Assert
        result.IsValid.Should().BeFalse();
    }

    [Fact]
    public void Validate_SkuWithValidFormat_NoError()
    {
        // Arrange
        var command = new CreateProductCommand(
            CategoryId: Guid.NewGuid(),
            Name: "Valid",
            Sku: "VALID-SKU-001");

        // Act
        var result = _validator.Validate(command);

        // Assert
        result.IsValid.Should().BeTrue();
    }

    #endregion

    #region Slug Format Tests

    [Fact]
    public void Validate_CustomSlugWithInvalidFormat_HasError()
    {
        // Arrange
        var command = new CreateProductCommand(
            CategoryId: Guid.NewGuid(),
            Name: "Valid",
            Sku: "VALID-006",
            CustomSlug: "INVALID-UPPERCASE");

        // Act
        var result = _validator.Validate(command);

        // Assert
        result.IsValid.Should().BeFalse();
    }

    [Fact]
    public void Validate_CustomSlugWithValidFormat_NoError()
    {
        // Arrange
        var command = new CreateProductCommand(
            CategoryId: Guid.NewGuid(),
            Name: "Valid",
            Sku: "VALID-007",
            CustomSlug: "valid-slug-format");

        // Act
        var result = _validator.Validate(command);

        // Assert
        result.IsValid.Should().BeTrue();
    }

    #endregion

    #region Numeric Validation Tests

    [Fact]
    public void Validate_NegativePrice_HasError()
    {
        // Arrange
        var command = new CreateProductCommand(
            CategoryId: Guid.NewGuid(),
            Name: "Valid",
            Sku: "VALID-008",
            Price: -10);

        // Act
        var result = _validator.Validate(command);

        // Assert
        result.IsValid.Should().BeFalse();
    }

    [Fact]
    public void Validate_ComparePriceLessThanPrice_HasError()
    {
        // Arrange
        var command = new CreateProductCommand(
            CategoryId: Guid.NewGuid(),
            Name: "Valid",
            Sku: "VALID-009",
            Price: 100,
            CompareAtPrice: 50);

        // Act
        var result = _validator.Validate(command);

        // Assert
        result.IsValid.Should().BeFalse();
    }

    [Fact]
    public void Validate_NegativeWeight_HasError()
    {
        // Arrange
        var command = new CreateProductCommand(
            CategoryId: Guid.NewGuid(),
            Name: "Valid",
            Sku: "VALID-010",
            Weight: -1);

        // Act
        var result = _validator.Validate(command);

        // Assert
        result.IsValid.Should().BeFalse();
    }

    [Fact]
    public void Validate_NegativeDimension_HasError()
    {
        // Arrange
        var command = new CreateProductCommand(
            CategoryId: Guid.NewGuid(),
            Name: "Valid",
            Sku: "VALID-011",
            Length: -5);

        // Act
        var result = _validator.Validate(command);

        // Assert
        result.IsValid.Should().BeFalse();
    }

    #endregion

    #region Collection Constraint Tests

    [Fact]
    public void Validate_TooManyAttributes_HasError()
    {
        // Arrange
        var attributes = Enumerable.Range(1, 51)
            .ToDictionary(i => $"Attr{i}", i => "Value");

        var command = new CreateProductCommand(
            CategoryId: Guid.NewGuid(),
            Name: "Valid",
            Sku: "VALID-012",
            Attributes: attributes);

        // Act
        var result = _validator.Validate(command);

        // Assert
        result.IsValid.Should().BeFalse();
    }

    [Fact]
    public void Validate_TooManyTags_HasError()
    {
        // Arrange
        var tags = Enumerable.Range(1, 21)
            .Select(i => $"Tag{i}")
            .ToList();

        var command = new CreateProductCommand(
            CategoryId: Guid.NewGuid(),
            Name: "Valid",
            Sku: "VALID-013",
            Tags: tags);

        // Act
        var result = _validator.Validate(command);

        // Assert
        result.IsValid.Should().BeFalse();
    }

    #endregion

    #region Valid Command Tests

    [Fact]
    public void Validate_ValidCommand_NoErrors()
    {
        // Arrange
        var command = new CreateProductCommand(
            CategoryId: Guid.NewGuid(),
            Name: "Valid Product",
            Sku: "VALID-FINAL");

        // Act
        var result = _validator.Validate(command);

        // Assert
        result.IsValid.Should().BeTrue();
        result.Errors.Should().BeEmpty();
    }

    [Fact]
    public void Validate_ValidCommandWithAllFields_NoErrors()
    {
        // Arrange
        var command = new CreateProductCommand(
            CategoryId: Guid.NewGuid(),
            Name: "Complete Product",
            Sku: "COMPLETE-001",
            CustomSlug: "complete-product",
            ShortDescription: "A complete product",
            Description: "This is a complete product with all fields",
            ProductType: "Physical",
            Status: "Draft",
            Price: 99.99m,
            CompareAtPrice: 149.99m,
            CostPrice: 50.00m,
            Weight: 2.5m,
            Length: 10m,
            Width: 5m,
            Height: 3m,
            IsFeatured: true,
            TrackInventory: true,
            Taxable: true,
            Attributes: new Dictionary<string, string> { { "Color", "Blue" } },
            Tags: new List<string> { "Sale", "Featured" });

        // Act
        var result = _validator.Validate(command);

        // Assert
        result.IsValid.Should().BeTrue();
    }

    #endregion
}
