using FluentAssertions;
using KromicStore.Application.Features.Shopping.Commands.AddToCart;
using Xunit;

namespace KromicStore.Application.Tests.Features.Shopping.Commands.Cart;

/// <summary>
/// Validator tests for AddToCartCommand.
/// Verifies validation rules for adding items to cart.
/// </summary>
public sealed class AddToCartCommandValidatorTests
{
    private readonly AddToCartCommandValidator _validator;

    public AddToCartCommandValidatorTests()
    {
        _validator = new AddToCartCommandValidator();
    }

    #region Cart ID Validation Tests

    [Fact]
    public void Validate_EmptyCartId_HasError()
    {
        // Arrange
        var command = new AddToCartCommand(
            CartId: Guid.Empty,
            ProductId: Guid.NewGuid(),
            UnitPrice: 50m,
            Quantity: 1);

        // Act
        var result = _validator.Validate(command);

        // Assert
        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.PropertyName == "CartId");
    }

    [Fact]
    public void Validate_ValidCartId_NoError()
    {
        // Arrange
        var command = new AddToCartCommand(
            CartId: Guid.NewGuid(),
            ProductId: Guid.NewGuid(),
            UnitPrice: 50m,
            Quantity: 1);

        // Act
        var result = _validator.Validate(command);

        // Assert
        result.Errors.Should().NotContain(e => e.PropertyName == "CartId");
    }

    #endregion

    #region Product ID Validation Tests

    [Fact]
    public void Validate_EmptyProductId_HasError()
    {
        // Arrange
        var command = new AddToCartCommand(
            CartId: Guid.NewGuid(),
            ProductId: Guid.Empty,
            UnitPrice: 50m,
            Quantity: 1);

        // Act
        var result = _validator.Validate(command);

        // Assert
        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.PropertyName == "ProductId");
    }

    [Fact]
    public void Validate_ValidProductId_NoError()
    {
        // Arrange
        var command = new AddToCartCommand(
            CartId: Guid.NewGuid(),
            ProductId: Guid.NewGuid(),
            UnitPrice: 50m,
            Quantity: 1);

        // Act
        var result = _validator.Validate(command);

        // Assert
        result.Errors.Should().NotContain(e => e.PropertyName == "ProductId");
    }

    #endregion

    #region Unit Price Validation Tests

    [Fact]
    public void Validate_NegativePrice_HasError()
    {
        // Arrange
        var command = new AddToCartCommand(
            CartId: Guid.NewGuid(),
            ProductId: Guid.NewGuid(),
            UnitPrice: -10m,
            Quantity: 1);

        // Act
        var result = _validator.Validate(command);

        // Assert
        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.PropertyName == "UnitPrice");
    }

    [Fact]
    public void Validate_ZeroPrice_NoError()
    {
        // Arrange
        var command = new AddToCartCommand(
            CartId: Guid.NewGuid(),
            ProductId: Guid.NewGuid(),
            UnitPrice: 0m,
            Quantity: 1);

        // Act
        var result = _validator.Validate(command);

        // Assert
        result.Errors.Should().NotContain(e => e.PropertyName == "UnitPrice");
    }

    [Fact]
    public void Validate_ValidPrice_NoError()
    {
        // Arrange
        var command = new AddToCartCommand(
            CartId: Guid.NewGuid(),
            ProductId: Guid.NewGuid(),
            UnitPrice: 99.99m,
            Quantity: 1);

        // Act
        var result = _validator.Validate(command);

        // Assert
        result.Errors.Should().NotContain(e => e.PropertyName == "UnitPrice");
    }

    [Fact]
    public void Validate_LargePrice_NoError()
    {
        // Arrange
        var command = new AddToCartCommand(
            CartId: Guid.NewGuid(),
            ProductId: Guid.NewGuid(),
            UnitPrice: 9999999.99m,
            Quantity: 1);

        // Act
        var result = _validator.Validate(command);

        // Assert
        result.Errors.Should().NotContain(e => e.PropertyName == "UnitPrice");
    }

    [Fact]
    public void Validate_PriceExceedsMaxAllowed_HasError()
    {
        // Arrange
        var command = new AddToCartCommand(
            CartId: Guid.NewGuid(),
            ProductId: Guid.NewGuid(),
            UnitPrice: decimal.MaxValue,
            Quantity: 1);

        // Act
        var result = _validator.Validate(command);

        // Assert
        result.IsValid.Should().BeFalse();
    }

    #endregion

    #region Quantity Validation Tests

    [Fact]
    public void Validate_ZeroQuantity_HasError()
    {
        // Arrange
        var command = new AddToCartCommand(
            CartId: Guid.NewGuid(),
            ProductId: Guid.NewGuid(),
            UnitPrice: 50m,
            Quantity: 0);

        // Act
        var result = _validator.Validate(command);

        // Assert
        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.PropertyName == "Quantity");
    }

    [Fact]
    public void Validate_NegativeQuantity_HasError()
    {
        // Arrange
        var command = new AddToCartCommand(
            CartId: Guid.NewGuid(),
            ProductId: Guid.NewGuid(),
            UnitPrice: 50m,
            Quantity: -1);

        // Act
        var result = _validator.Validate(command);

        // Assert
        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.PropertyName == "Quantity");
    }

    [Fact]
    public void Validate_QuantityOne_NoError()
    {
        // Arrange
        var command = new AddToCartCommand(
            CartId: Guid.NewGuid(),
            ProductId: Guid.NewGuid(),
            UnitPrice: 50m,
            Quantity: 1);

        // Act
        var result = _validator.Validate(command);

        // Assert
        result.Errors.Should().NotContain(e => e.PropertyName == "Quantity");
    }

    [Fact]
    public void Validate_LargeQuantity_NoError()
    {
        // Arrange
        var command = new AddToCartCommand(
            CartId: Guid.NewGuid(),
            ProductId: Guid.NewGuid(),
            UnitPrice: 50m,
            Quantity: 999);

        // Act
        var result = _validator.Validate(command);

        // Assert
        result.Errors.Should().NotContain(e => e.PropertyName == "Quantity");
    }

    [Fact]
    public void Validate_QuantityExceedsMax_HasError()
    {
        // Arrange
        var command = new AddToCartCommand(
            CartId: Guid.NewGuid(),
            ProductId: Guid.NewGuid(),
            UnitPrice: 50m,
            Quantity: 1001);

        // Act
        var result = _validator.Validate(command);

        // Assert
        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.PropertyName == "Quantity");
    }

    #endregion

    #region Variant ID Validation Tests

    [Fact]
    public void Validate_WithVariantId_NoError()
    {
        // Arrange
        var command = new AddToCartCommand(
            CartId: Guid.NewGuid(),
            ProductId: Guid.NewGuid(),
            UnitPrice: 50m,
            Quantity: 1,
            VariantId: Guid.NewGuid());

        // Act
        var result = _validator.Validate(command);

        // Assert
        result.IsValid.Should().BeTrue();
    }

    [Fact]
    public void Validate_WithoutVariantId_NoError()
    {
        // Arrange
        var command = new AddToCartCommand(
            CartId: Guid.NewGuid(),
            ProductId: Guid.NewGuid(),
            UnitPrice: 50m,
            Quantity: 1);

        // Act
        var result = _validator.Validate(command);

        // Assert
        result.IsValid.Should().BeTrue();
    }

    #endregion

    #region Valid Command Tests

    [Fact]
    public void Validate_ValidCommand_NoErrors()
    {
        // Arrange
        var command = new AddToCartCommand(
            CartId: Guid.NewGuid(),
            ProductId: Guid.NewGuid(),
            UnitPrice: 50m,
            Quantity: 2);

        // Act
        var result = _validator.Validate(command);

        // Assert
        result.IsValid.Should().BeTrue();
        result.Errors.Should().BeEmpty();
    }

    [Fact]
    public void Validate_ValidCommandWithVariant_NoErrors()
    {
        // Arrange
        var command = new AddToCartCommand(
            CartId: Guid.NewGuid(),
            ProductId: Guid.NewGuid(),
            UnitPrice: 75.50m,
            Quantity: 3,
            VariantId: Guid.NewGuid());

        // Act
        var result = _validator.Validate(command);

        // Assert
        result.IsValid.Should().BeTrue();
        result.Errors.Should().BeEmpty();
    }

    #endregion
}
