using FluentAssertions;
using KromicStore.Application.Features.Shopping.Commands.RemoveCartItem;
using Xunit;

namespace KromicStore.Application.Tests.Features.Shopping.Commands.Cart;

/// <summary>
/// Validator tests for RemoveCartItemCommand.
/// Verifies validation rules for removing items from cart.
/// </summary>
public sealed class RemoveCartItemCommandValidatorTests
{
    private readonly RemoveCartItemCommandValidator _validator;

    public RemoveCartItemCommandValidatorTests()
    {
        _validator = new RemoveCartItemCommandValidator();
    }

    #region Cart ID Validation Tests

    [Fact]
    public void Validate_EmptyCartId_HasError()
    {
        // Arrange
        var command = new RemoveCartItemCommand(
            CartId: Guid.Empty,
            ProductId: Guid.NewGuid());

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
        var command = new RemoveCartItemCommand(
            CartId: Guid.NewGuid(),
            ProductId: Guid.NewGuid());

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
        var command = new RemoveCartItemCommand(
            CartId: Guid.NewGuid(),
            ProductId: Guid.Empty);

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
        var command = new RemoveCartItemCommand(
            CartId: Guid.NewGuid(),
            ProductId: Guid.NewGuid());

        // Act
        var result = _validator.Validate(command);

        // Assert
        result.Errors.Should().NotContain(e => e.PropertyName == "ProductId");
    }

    #endregion

    #region Variant ID Validation Tests

    [Fact]
    public void Validate_WithVariantId_NoError()
    {
        // Arrange
        var command = new RemoveCartItemCommand(
            CartId: Guid.NewGuid(),
            ProductId: Guid.NewGuid(),
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
        var command = new RemoveCartItemCommand(
            CartId: Guid.NewGuid(),
            ProductId: Guid.NewGuid());

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
        var command = new RemoveCartItemCommand(
            CartId: Guid.NewGuid(),
            ProductId: Guid.NewGuid());

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
        var command = new RemoveCartItemCommand(
            CartId: Guid.NewGuid(),
            ProductId: Guid.NewGuid(),
            VariantId: Guid.NewGuid());

        // Act
        var result = _validator.Validate(command);

        // Assert
        result.IsValid.Should().BeTrue();
        result.Errors.Should().BeEmpty();
    }

    #endregion
}
