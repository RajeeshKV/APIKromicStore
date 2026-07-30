using FluentAssertions;
using KromicStore.Application.Features.Shopping.Commands.ClearCart;
using Xunit;

namespace KromicStore.Application.Tests.Features.Shopping.Commands.Cart;

/// <summary>
/// Validator tests for ClearCartCommand.
/// Verifies validation rules for clearing cart.
/// </summary>
public sealed class ClearCartCommandValidatorTests
{
    private readonly ClearCartCommandValidator _validator;

    public ClearCartCommandValidatorTests()
    {
        _validator = new ClearCartCommandValidator();
    }

    #region Cart ID Validation Tests

    [Fact]
    public void Validate_EmptyCartId_HasError()
    {
        // Arrange
        var command = new ClearCartCommand(CartId: Guid.Empty);

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
        var command = new ClearCartCommand(CartId: Guid.NewGuid());

        // Act
        var result = _validator.Validate(command);

        // Assert
        result.IsValid.Should().BeTrue();
        result.Errors.Should().BeEmpty();
    }

    #endregion

    #region Valid Command Tests

    [Fact]
    public void Validate_ValidCommand_NoErrors()
    {
        // Arrange
        var command = new ClearCartCommand(CartId: Guid.NewGuid());

        // Act
        var result = _validator.Validate(command);

        // Assert
        result.IsValid.Should().BeTrue();
        result.Errors.Should().BeEmpty();
    }

    [Fact]
    public void Validate_MultipleValidCartIds_AllSucceed()
    {
        // Arrange
        var cartIds = new[] 
        { 
            Guid.NewGuid(), 
            Guid.NewGuid(), 
            Guid.NewGuid() 
        };

        // Act & Assert
        foreach (var cartId in cartIds)
        {
            var command = new ClearCartCommand(CartId: cartId);
            var result = _validator.Validate(command);
            result.IsValid.Should().BeTrue();
        }
    }

    #endregion
}
