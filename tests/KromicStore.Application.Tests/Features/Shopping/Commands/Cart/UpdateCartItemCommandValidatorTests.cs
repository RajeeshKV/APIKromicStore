using FluentAssertions;
using KromicStore.Application.Features.Shopping.Commands.UpdateCartItem;
using Xunit;

namespace KromicStore.Application.Tests.Features.Shopping.Commands.Cart;

/// <summary>
/// Validator tests for UpdateCartItemCommand.
/// Verifies validation rules for updating cart item quantities.
/// </summary>
public sealed class UpdateCartItemCommandValidatorTests
{
    private readonly UpdateCartItemCommandValidator _validator;

    public UpdateCartItemCommandValidatorTests()
    {
        _validator = new UpdateCartItemCommandValidator();
    }

    #region Cart ID Validation Tests

    [Fact]
    public void Validate_EmptyCartId_HasError()
    {
        // Arrange
        var command = new UpdateCartItemCommand(
            CartId: Guid.Empty,
            ProductId: Guid.NewGuid(),
            NewQuantity: 2);

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
        var command = new UpdateCartItemCommand(
            CartId: Guid.NewGuid(),
            ProductId: Guid.NewGuid(),
            NewQuantity: 2);

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
        var command = new UpdateCartItemCommand(
            CartId: Guid.NewGuid(),
            ProductId: Guid.Empty,
            NewQuantity: 2);

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
        var command = new UpdateCartItemCommand(
            CartId: Guid.NewGuid(),
            ProductId: Guid.NewGuid(),
            NewQuantity: 2);

        // Act
        var result = _validator.Validate(command);

        // Assert
        result.Errors.Should().NotContain(e => e.PropertyName == "ProductId");
    }

    #endregion

    #region Quantity Validation Tests

    [Fact]
    public void Validate_NegativeQuantity_HasError()
    {
        // Arrange
        var command = new UpdateCartItemCommand(
            CartId: Guid.NewGuid(),
            ProductId: Guid.NewGuid(),
            NewQuantity: -1);

        // Act
        var result = _validator.Validate(command);

        // Assert
        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.PropertyName == "NewQuantity");
    }

    [Fact]
    public void Validate_ZeroQuantity_NoError()
    {
        // Arrange
        var command = new UpdateCartItemCommand(
            CartId: Guid.NewGuid(),
            ProductId: Guid.NewGuid(),
            NewQuantity: 0);

        // Act
        var result = _validator.Validate(command);

        // Assert
        result.Errors.Should().NotContain(e => e.PropertyName == "NewQuantity");
    }

    [Fact]
    public void Validate_QuantityOne_NoError()
    {
        // Arrange
        var command = new UpdateCartItemCommand(
            CartId: Guid.NewGuid(),
            ProductId: Guid.NewGuid(),
            NewQuantity: 1);

        // Act
        var result = _validator.Validate(command);

        // Assert
        result.Errors.Should().NotContain(e => e.PropertyName == "NewQuantity");
    }

    [Fact]
    public void Validate_LargeQuantity_NoError()
    {
        // Arrange
        var command = new UpdateCartItemCommand(
            CartId: Guid.NewGuid(),
            ProductId: Guid.NewGuid(),
            NewQuantity: 999);

        // Act
        var result = _validator.Validate(command);

        // Assert
        result.Errors.Should().NotContain(e => e.PropertyName == "NewQuantity");
    }

    [Fact]
    public void Validate_QuantityExceedsMax_HasError()
    {
        // Arrange
        var command = new UpdateCartItemCommand(
            CartId: Guid.NewGuid(),
            ProductId: Guid.NewGuid(),
            NewQuantity: 1001);

        // Act
        var result = _validator.Validate(command);

        // Assert
        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.PropertyName == "NewQuantity");
    }

    #endregion

    #region Variant ID Validation Tests

    [Fact]
    public void Validate_WithVariantId_NoError()
    {
        // Arrange
        var command = new UpdateCartItemCommand(
            CartId: Guid.NewGuid(),
            ProductId: Guid.NewGuid(),
            NewQuantity: 2,
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
        var command = new UpdateCartItemCommand(
            CartId: Guid.NewGuid(),
            ProductId: Guid.NewGuid(),
            NewQuantity: 2);

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
        var command = new UpdateCartItemCommand(
            CartId: Guid.NewGuid(),
            ProductId: Guid.NewGuid(),
            NewQuantity: 3);

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
        var command = new UpdateCartItemCommand(
            CartId: Guid.NewGuid(),
            ProductId: Guid.NewGuid(),
            NewQuantity: 5,
            VariantId: Guid.NewGuid());

        // Act
        var result = _validator.Validate(command);

        // Assert
        result.IsValid.Should().BeTrue();
        result.Errors.Should().BeEmpty();
    }

    [Fact]
    public void Validate_ValidCommandRemoveItem_NoErrors()
    {
        // Arrange
        var command = new UpdateCartItemCommand(
            CartId: Guid.NewGuid(),
            ProductId: Guid.NewGuid(),
            NewQuantity: 0);

        // Act
        var result = _validator.Validate(command);

        // Assert
        result.IsValid.Should().BeTrue();
        result.Errors.Should().BeEmpty();
    }

    #endregion
}
