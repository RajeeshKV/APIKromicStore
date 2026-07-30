using FluentAssertions;
using FluentValidation.TestHelper;
using KromicStore.Application.Features.Shopping.Commands.AddToWishlist;
using KromicStore.Application.Features.Shopping.Commands.CreateWishlist;
using KromicStore.Application.Features.Shopping.Commands.RemoveFromWishlist;
using Xunit;

namespace KromicStore.Application.Tests.Features.Shopping.Commands.Wishlist;

/// <summary>
/// Tests for all Wishlist command validators.
/// Verifies input validation for CreateWishlist, AddToWishlist, and RemoveFromWishlist commands.
/// </summary>
public sealed class WishlistCommandValidatorTests
{
    #region CreateWishlistCommandValidator Tests

    public sealed class CreateWishlistCommandValidatorTests
    {
        private readonly CreateWishlistCommandValidator _validator;

        public CreateWishlistCommandValidatorTests()
        {
            _validator = new CreateWishlistCommandValidator();
        }

        #region Valid Data Tests

        [Fact]
        public void Validate_WithValidCustomerId_IsValid()
        {
            // Arrange
            var command = new CreateWishlistCommand(CustomerId: Guid.NewGuid());

            // Act
            var result = _validator.TestValidate(command);

            // Assert
            result.ShouldNotHaveAnyValidationErrors();
        }

        [Fact]
        public void Validate_WithDifferentValidCustomerIds_AllValid()
        {
            // Arrange
            var customerId1 = Guid.NewGuid();
            var customerId2 = Guid.NewGuid();
            var customerId3 = Guid.NewGuid();

            var command1 = new CreateWishlistCommand(CustomerId: customerId1);
            var command2 = new CreateWishlistCommand(CustomerId: customerId2);
            var command3 = new CreateWishlistCommand(CustomerId: customerId3);

            // Act & Assert
            _validator.TestValidate(command1).ShouldNotHaveAnyValidationErrors();
            _validator.TestValidate(command2).ShouldNotHaveAnyValidationErrors();
            _validator.TestValidate(command3).ShouldNotHaveAnyValidationErrors();
        }

        #endregion

        #region Invalid Data Tests

        [Fact]
        public void Validate_WithEmptyCustomerId_IsInvalid()
        {
            // Arrange
            var command = new CreateWishlistCommand(CustomerId: Guid.Empty);

            // Act
            var result = _validator.TestValidate(command);

            // Assert
            result.ShouldHaveValidationErrorFor(c => c.CustomerId);
        }

        #endregion

        #region Edge Cases Tests

        [Fact]
        public void Validate_WithMaxGuid_IsValid()
        {
            // Arrange
            var command = new CreateWishlistCommand(CustomerId: Guid.Parse("ffffffff-ffff-ffff-ffff-ffffffffffff"));

            // Act
            var result = _validator.TestValidate(command);

            // Assert
            result.ShouldNotHaveAnyValidationErrors();
        }

        #endregion
    }

    #endregion

    #region AddToWishlistCommandValidator Tests

    public sealed class AddToWishlistCommandValidatorTests
    {
        private readonly AddToWishlistCommandValidator _validator;

        public AddToWishlistCommandValidatorTests()
        {
            _validator = new AddToWishlistCommandValidator();
        }

        #region Valid Data Tests

        [Fact]
        public void Validate_WithValidWishlistAndProductId_IsValid()
        {
            // Arrange
            var command = new AddToWishlistCommand(
                WishlistId: Guid.NewGuid(),
                ProductId: Guid.NewGuid());

            // Act
            var result = _validator.TestValidate(command);

            // Assert
            result.ShouldNotHaveAnyValidationErrors();
        }

        [Fact]
        public void Validate_WithValidIds_AndVariantId_IsValid()
        {
            // Arrange
            var command = new AddToWishlistCommand(
                WishlistId: Guid.NewGuid(),
                ProductId: Guid.NewGuid(),
                VariantId: Guid.NewGuid());

            // Act
            var result = _validator.TestValidate(command);

            // Assert
            result.ShouldNotHaveAnyValidationErrors();
        }

        [Fact]
        public void Validate_WithValidIds_AndNullVariantId_IsValid()
        {
            // Arrange
            var command = new AddToWishlistCommand(
                WishlistId: Guid.NewGuid(),
                ProductId: Guid.NewGuid(),
                VariantId: null);

            // Act
            var result = _validator.TestValidate(command);

            // Assert
            result.ShouldNotHaveAnyValidationErrors();
        }

        [Fact]
        public void Validate_WithDifferentValidIds_AllValid()
        {
            // Arrange
            var commands = Enumerable.Range(0, 5)
                .Select(_ => new AddToWishlistCommand(
                    WishlistId: Guid.NewGuid(),
                    ProductId: Guid.NewGuid()))
                .ToList();

            // Act & Assert
            foreach (var command in commands)
            {
                _validator.TestValidate(command).ShouldNotHaveAnyValidationErrors();
            }
        }

        #endregion

        #region Invalid Data Tests

        [Fact]
        public void Validate_WithEmptyWishlistId_IsInvalid()
        {
            // Arrange
            var command = new AddToWishlistCommand(
                WishlistId: Guid.Empty,
                ProductId: Guid.NewGuid());

            // Act
            var result = _validator.TestValidate(command);

            // Assert
            result.ShouldHaveValidationErrorFor(c => c.WishlistId);
        }

        [Fact]
        public void Validate_WithEmptyProductId_IsInvalid()
        {
            // Arrange
            var command = new AddToWishlistCommand(
                WishlistId: Guid.NewGuid(),
                ProductId: Guid.Empty);

            // Act
            var result = _validator.TestValidate(command);

            // Assert
            result.ShouldHaveValidationErrorFor(c => c.ProductId);
        }

        [Fact]
        public void Validate_WithBothEmptyIds_IsInvalid()
        {
            // Arrange
            var command = new AddToWishlistCommand(
                WishlistId: Guid.Empty,
                ProductId: Guid.Empty);

            // Act
            var result = _validator.TestValidate(command);

            // Assert
            result.ShouldHaveValidationErrorFor(c => c.WishlistId);
            result.ShouldHaveValidationErrorFor(c => c.ProductId);
        }

        [Fact]
        public void Validate_WithEmptyWishlistIdValidProductIdAndVariantId_IsInvalid()
        {
            // Arrange
            var command = new AddToWishlistCommand(
                WishlistId: Guid.Empty,
                ProductId: Guid.NewGuid(),
                VariantId: Guid.NewGuid());

            // Act
            var result = _validator.TestValidate(command);

            // Assert
            result.ShouldHaveValidationErrorFor(c => c.WishlistId);
        }

        [Fact]
        public void Validate_WithValidWishlistIdEmptyProductIdAndVariantId_IsInvalid()
        {
            // Arrange
            var command = new AddToWishlistCommand(
                WishlistId: Guid.NewGuid(),
                ProductId: Guid.Empty,
                VariantId: Guid.NewGuid());

            // Act
            var result = _validator.TestValidate(command);

            // Assert
            result.ShouldHaveValidationErrorFor(c => c.ProductId);
        }

        #endregion

        #region Edge Cases Tests

        [Fact]
        public void Validate_WithMaxGuids_IsValid()
        {
            // Arrange
            var maxGuid = Guid.Parse("ffffffff-ffff-ffff-ffff-ffffffffffff");
            var command = new AddToWishlistCommand(
                WishlistId: maxGuid,
                ProductId: maxGuid,
                VariantId: maxGuid);

            // Act
            var result = _validator.TestValidate(command);

            // Assert
            result.ShouldNotHaveAnyValidationErrors();
        }

        [Fact]
        public void Validate_WithMinGuids_OneIsEmpty_IsInvalid()
        {
            // Arrange
            var command = new AddToWishlistCommand(
                WishlistId: Guid.Empty,
                ProductId: Guid.NewGuid());

            // Act
            var result = _validator.TestValidate(command);

            // Assert
            result.ShouldHaveValidationErrorFor(c => c.WishlistId);
        }

        #endregion
    }

    #endregion

    #region RemoveFromWishlistCommandValidator Tests

    public sealed class RemoveFromWishlistCommandValidatorTests
    {
        private readonly RemoveFromWishlistCommandValidator _validator;

        public RemoveFromWishlistCommandValidatorTests()
        {
            _validator = new RemoveFromWishlistCommandValidator();
        }

        #region Valid Data Tests

        [Fact]
        public void Validate_WithValidWishlistAndProductId_IsValid()
        {
            // Arrange
            var command = new RemoveFromWishlistCommand(
                WishlistId: Guid.NewGuid(),
                ProductId: Guid.NewGuid());

            // Act
            var result = _validator.TestValidate(command);

            // Assert
            result.ShouldNotHaveAnyValidationErrors();
        }

        [Fact]
        public void Validate_WithValidIds_AndVariantId_IsValid()
        {
            // Arrange
            var command = new RemoveFromWishlistCommand(
                WishlistId: Guid.NewGuid(),
                ProductId: Guid.NewGuid(),
                VariantId: Guid.NewGuid());

            // Act
            var result = _validator.TestValidate(command);

            // Assert
            result.ShouldNotHaveAnyValidationErrors();
        }

        [Fact]
        public void Validate_WithValidIds_AndNullVariantId_IsValid()
        {
            // Arrange
            var command = new RemoveFromWishlistCommand(
                WishlistId: Guid.NewGuid(),
                ProductId: Guid.NewGuid(),
                VariantId: null);

            // Act
            var result = _validator.TestValidate(command);

            // Assert
            result.ShouldNotHaveAnyValidationErrors();
        }

        [Fact]
        public void Validate_WithDifferentValidIds_AllValid()
        {
            // Arrange
            var commands = Enumerable.Range(0, 5)
                .Select(_ => new RemoveFromWishlistCommand(
                    WishlistId: Guid.NewGuid(),
                    ProductId: Guid.NewGuid()))
                .ToList();

            // Act & Assert
            foreach (var command in commands)
            {
                _validator.TestValidate(command).ShouldNotHaveAnyValidationErrors();
            }
        }

        #endregion

        #region Invalid Data Tests

        [Fact]
        public void Validate_WithEmptyWishlistId_IsInvalid()
        {
            // Arrange
            var command = new RemoveFromWishlistCommand(
                WishlistId: Guid.Empty,
                ProductId: Guid.NewGuid());

            // Act
            var result = _validator.TestValidate(command);

            // Assert
            result.ShouldHaveValidationErrorFor(c => c.WishlistId);
        }

        [Fact]
        public void Validate_WithEmptyProductId_IsInvalid()
        {
            // Arrange
            var command = new RemoveFromWishlistCommand(
                WishlistId: Guid.NewGuid(),
                ProductId: Guid.Empty);

            // Act
            var result = _validator.TestValidate(command);

            // Assert
            result.ShouldHaveValidationErrorFor(c => c.ProductId);
        }

        [Fact]
        public void Validate_WithBothEmptyIds_IsInvalid()
        {
            // Arrange
            var command = new RemoveFromWishlistCommand(
                WishlistId: Guid.Empty,
                ProductId: Guid.Empty);

            // Act
            var result = _validator.TestValidate(command);

            // Assert
            result.ShouldHaveValidationErrorFor(c => c.WishlistId);
            result.ShouldHaveValidationErrorFor(c => c.ProductId);
        }

        [Fact]
        public void Validate_WithEmptyWishlistIdValidProductIdAndVariantId_IsInvalid()
        {
            // Arrange
            var command = new RemoveFromWishlistCommand(
                WishlistId: Guid.Empty,
                ProductId: Guid.NewGuid(),
                VariantId: Guid.NewGuid());

            // Act
            var result = _validator.TestValidate(command);

            // Assert
            result.ShouldHaveValidationErrorFor(c => c.WishlistId);
        }

        [Fact]
        public void Validate_WithValidWishlistIdEmptyProductIdAndVariantId_IsInvalid()
        {
            // Arrange
            var command = new RemoveFromWishlistCommand(
                WishlistId: Guid.NewGuid(),
                ProductId: Guid.Empty,
                VariantId: Guid.NewGuid());

            // Act
            var result = _validator.TestValidate(command);

            // Assert
            result.ShouldHaveValidationErrorFor(c => c.ProductId);
        }

        #endregion

        #region Edge Cases Tests

        [Fact]
        public void Validate_WithMaxGuids_IsValid()
        {
            // Arrange
            var maxGuid = Guid.Parse("ffffffff-ffff-ffff-ffff-ffffffffffff");
            var command = new RemoveFromWishlistCommand(
                WishlistId: maxGuid,
                ProductId: maxGuid,
                VariantId: maxGuid);

            // Act
            var result = _validator.TestValidate(command);

            // Assert
            result.ShouldNotHaveAnyValidationErrors();
        }

        [Fact]
        public void Validate_WithEmptyVariantId_IsValid()
        {
            // Arrange
            var command = new RemoveFromWishlistCommand(
                WishlistId: Guid.NewGuid(),
                ProductId: Guid.NewGuid(),
                VariantId: Guid.Empty);

            // Act
            var result = _validator.TestValidate(command);

            // Assert
            result.ShouldNotHaveAnyValidationErrors();
        }

        #endregion
    }

    #endregion
}
