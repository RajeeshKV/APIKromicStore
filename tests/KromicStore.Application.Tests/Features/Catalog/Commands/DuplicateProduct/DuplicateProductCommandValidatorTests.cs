using FluentAssertions;
using KromicStore.Application.Features.Catalog.Commands.DuplicateProduct;
using KromicStore.Application.Features.Catalog.Abstractions;
using NSubstitute;
using Xunit;

#pragma warning disable CS8625

namespace KromicStore.Application.Tests.Features.Catalog.Commands.DuplicateProduct;

/// <summary>
/// Validator tests for DuplicateProductCommand.
/// Verifies all validation rules for product duplication.
/// </summary>
public sealed class DuplicateProductCommandValidatorTests
{
    private readonly IProductRepository _productRepository;
    private readonly DuplicateProductCommandValidator _validator;

    public DuplicateProductCommandValidatorTests()
    {
        _productRepository = Substitute.For<IProductRepository>();
        _validator = new DuplicateProductCommandValidator(_productRepository);
    }

    [Fact]
    public async Task Validate_ProductIdEmpty_IsInvalid()
    {
        // Arrange
        var command = new DuplicateProductCommand(
            ProductId: Guid.Empty,
            NewSku: "NEW-SKU",
            NewName: "New");

        // Act
        var result = await _validator.ValidateAsync(command);

        // Assert
        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.PropertyName == "ProductId");
    }

    [Fact]
    public async Task Validate_NewSkuEmpty_IsInvalid()
    {
        // Arrange
        var command = new DuplicateProductCommand(
            ProductId: Guid.NewGuid(),
            NewSku: "",
            NewName: "New");

        // Act
        var result = await _validator.ValidateAsync(command);

        // Assert
        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.PropertyName == "NewSku");
    }

    [Fact]
    public async Task Validate_NewSkuNull_IsInvalid()
    {
        // Arrange
        var command = new DuplicateProductCommand(
            ProductId: Guid.NewGuid(),
            NewSku: null,
            NewName: "New");

        // Act
        var result = await _validator.ValidateAsync(command);

        // Assert
        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.PropertyName == "NewSku");
    }

    [Fact]
    public async Task Validate_NewSkuExceeds50Chars_IsInvalid()
    {
        // Arrange
        var longSku = new string('A', 51);
        var command = new DuplicateProductCommand(
            ProductId: Guid.NewGuid(),
            NewSku: longSku,
            NewName: "New");

        // Act
        var result = await _validator.ValidateAsync(command);

        // Assert
        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.PropertyName == "NewSku" && e.ErrorMessage.Contains("50"));
    }

    [Fact]
    public async Task Validate_NewSkuInvalidFormat_IsInvalid()
    {
        // Arrange
        var command = new DuplicateProductCommand(
            ProductId: Guid.NewGuid(),
            NewSku: "invalid-lowercase-sku",
            NewName: "New");

        // Act
        var result = await _validator.ValidateAsync(command);

        // Assert
        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.PropertyName == "NewSku");
    }

    [Fact]
    public async Task Validate_ValidNewSku_IsValid()
    {
        // Arrange
        var command = new DuplicateProductCommand(
            ProductId: Guid.NewGuid(),
            NewSku: "VALID-SKU.123",
            NewName: "New");

        // Act
        var result = await _validator.ValidateAsync(command);

        // Assert
        result.Errors.Where(e => e.PropertyName == "NewSku").Should().BeEmpty();
    }

    [Fact]
    public async Task Validate_NewNameEmpty_IsInvalid()
    {
        // Arrange
        var command = new DuplicateProductCommand(
            ProductId: Guid.NewGuid(),
            NewSku: "NEW-SKU",
            NewName: "");

        // Act
        var result = await _validator.ValidateAsync(command);

        // Assert
        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.PropertyName == "NewName");
    }

    [Fact]
    public async Task Validate_NewNameNull_IsInvalid()
    {
        // Arrange
        var command = new DuplicateProductCommand(
            ProductId: Guid.NewGuid(),
            NewSku: "NEW-SKU",
            NewName: null);

        // Act
        var result = await _validator.ValidateAsync(command);

        // Assert
        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.PropertyName == "NewName");
    }

    [Fact]
    public async Task Validate_NewNameExceeds200Chars_IsInvalid()
    {
        // Arrange
        var longName = new string('A', 201);
        var command = new DuplicateProductCommand(
            ProductId: Guid.NewGuid(),
            NewSku: "NEW-SKU",
            NewName: longName);

        // Act
        var result = await _validator.ValidateAsync(command);

        // Assert
        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.PropertyName == "NewName" && e.ErrorMessage.Contains("200"));
    }

    [Fact]
    public async Task Validate_ValidNewName_IsValid()
    {
        // Arrange
        var command = new DuplicateProductCommand(
            ProductId: Guid.NewGuid(),
            NewSku: "NEW-SKU",
            NewName: "Valid Product Name");

        // Act
        var result = await _validator.ValidateAsync(command);

        // Assert
        result.Errors.Where(e => e.PropertyName == "NewName").Should().BeEmpty();
    }

    [Fact]
    public async Task Validate_NewSlugNull_IsValid()
    {
        // Arrange
        var command = new DuplicateProductCommand(
            ProductId: Guid.NewGuid(),
            NewSku: "NEW-SKU",
            NewName: "New",
            NewSlug: null);

        // Act
        var result = await _validator.ValidateAsync(command);

        // Assert
        result.Errors.Where(e => e.PropertyName == "NewSlug").Should().BeEmpty();
    }

    [Fact]
    public async Task Validate_NewSlugEmpty_IsValid()
    {
        // Arrange
        var command = new DuplicateProductCommand(
            ProductId: Guid.NewGuid(),
            NewSku: "NEW-SKU",
            NewName: "New",
            NewSlug: "");

        // Act
        var result = await _validator.ValidateAsync(command);

        // Assert
        result.Errors.Where(e => e.PropertyName == "NewSlug").Should().BeEmpty();
    }

    [Fact]
    public async Task Validate_NewSlugExceeds200Chars_IsInvalid()
    {
        // Arrange
        var longSlug = new string('a', 201);
        var command = new DuplicateProductCommand(
            ProductId: Guid.NewGuid(),
            NewSku: "NEW-SKU",
            NewName: "New",
            NewSlug: longSlug);

        // Act
        var result = await _validator.ValidateAsync(command);

        // Assert
        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.PropertyName == "NewSlug");
    }

    [Fact]
    public async Task Validate_NewSlugInvalidFormat_IsInvalid()
    {
        // Arrange
        var command = new DuplicateProductCommand(
            ProductId: Guid.NewGuid(),
            NewSku: "NEW-SKU",
            NewName: "New",
            NewSlug: "Invalid-Slug-With-Uppercase");

        // Act
        var result = await _validator.ValidateAsync(command);

        // Assert
        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.PropertyName == "NewSlug");
    }

    [Fact]
    public async Task Validate_ValidNewSlug_IsValid()
    {
        // Arrange
        var command = new DuplicateProductCommand(
            ProductId: Guid.NewGuid(),
            NewSku: "NEW-SKU",
            NewName: "New",
            NewSlug: "valid-slug-123");

        // Act
        var result = await _validator.ValidateAsync(command);

        // Assert
        result.Errors.Where(e => e.PropertyName == "NewSlug").Should().BeEmpty();
    }

    [Fact]
    public async Task Validate_AllRequiredFieldsProvided_IsValid()
    {
        // Arrange
        var command = new DuplicateProductCommand(
            ProductId: Guid.NewGuid(),
            NewSku: "NEW-SKU-001",
            NewName: "Duplicated Product");

        // Act
        var result = await _validator.ValidateAsync(command);

        // Assert
        result.IsValid.Should().BeTrue();
    }

    [Fact]
    public async Task Validate_AllFieldsProvidedWithSlug_IsValid()
    {
        // Arrange
        var command = new DuplicateProductCommand(
            ProductId: Guid.NewGuid(),
            NewSku: "NEW-SKU-002",
            NewName: "Duplicated Product With Slug",
            NewSlug: "duplicated-product-with-slug");

        // Act
        var result = await _validator.ValidateAsync(command);

        // Assert
        result.IsValid.Should().BeTrue();
    }
}
