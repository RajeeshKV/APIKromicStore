using FluentAssertions;
using KromicStore.Application.Common.Abstractions;
using KromicStore.Application.Features.Shopping.Commands.AddToCart;
using KromicStore.Application.Features.Shopping.Abstractions;
using KromicStore.Application.Tests.Features.Shopping.Common;
using KromicStore.Infrastructure.Persistence;
using Microsoft.Extensions.Logging;
using NSubstitute;
using Xunit;

namespace KromicStore.Application.Tests.Features.Shopping.Commands.Cart;

/// <summary>
/// Handler tests for AddToCartCommand.
/// Verifies adding items to cart with quantity management and validation.
/// </summary>
public sealed class AddToCartCommandHandlerTests
{
    private readonly ICartRepository _cartRepository;
    private readonly KromicStoreDbContext _dbContext;
    private readonly ICurrentUserService _currentUserService;
    private readonly ITenantContext _tenantContext;
    private readonly AddToCartCommandHandler _handler;
    private readonly Guid _tenantId;
    private readonly Guid _customerId;
    private readonly Guid _cartId;

    public AddToCartCommandHandlerTests()
    {
        _tenantId = Guid.NewGuid();
        _customerId = Guid.NewGuid();
        _cartId = Guid.NewGuid();
        _dbContext = ShoppingTestFixtures.CreateDbContext(_tenantId);
        _tenantContext = ShoppingTestFixtures.CreateTenantContext(_tenantId);
        _currentUserService = ShoppingTestFixtures.CreateCurrentUserService(_customerId);

        _cartRepository = Substitute.For<ICartRepository>();

        _handler = new AddToCartCommandHandler(
            _cartRepository,
            _dbContext,
            Substitute.For<ILogger<AddToCartCommandHandler>>(),
            _tenantContext,
            _currentUserService);
    }

    #region Add Item Tests

    [Fact]
    public async Task Handle_AddsItemToCart_WithValidData()
    {
        // Arrange
        var cart = ShoppingTestFixtures.CreateTestCustomerCart(_tenantId, _customerId, cartId: _cartId);
        _cartRepository.GetByIdAsync(_cartId, Arg.Any<CancellationToken>())
            .Returns(cart);

        var productId = Guid.NewGuid();
        var command = new AddToCartCommand(
            CartId: _cartId,
            ProductId: productId,
            UnitPrice: 99.99m,
            Quantity: 2);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result.CartId.Should().Be(_cartId);
        result.ProductId.Should().Be(productId);
        result.Quantity.Should().Be(2);
        result.UnitPrice.Should().Be(99.99m);
        result.LineTotal.Should().Be(199.98m);
    }

    [Fact]
    public async Task Handle_AddsItemToCart_CallsRepository_Update()
    {
        // Arrange
        var cart = ShoppingTestFixtures.CreateTestCustomerCart(_tenantId, _customerId, cartId: _cartId);
        _cartRepository.GetByIdAsync(_cartId, Arg.Any<CancellationToken>())
            .Returns(cart);

        var command = new AddToCartCommand(
            CartId: _cartId,
            ProductId: Guid.NewGuid(),
            UnitPrice: 50m,
            Quantity: 1);

        // Act
        await _handler.Handle(command, CancellationToken.None);

        // Assert
        _cartRepository.Received(1).Update(Arg.Any<Domain.Shopping.Entities.Cart>());
    }

    [Fact]
    public async Task Handle_MergesQuantity_IfItemAlreadyExists()
    {
        // Arrange
        var cart = ShoppingTestFixtures.CreateTestCustomerCart(_tenantId, _customerId, cartId: _cartId);
        var productId = Guid.NewGuid();
        cart.AddItem(productId, 50m, 2);
        
        _cartRepository.GetByIdAsync(_cartId, Arg.Any<CancellationToken>())
            .Returns(cart);

        var command = new AddToCartCommand(
            CartId: _cartId,
            ProductId: productId,
            UnitPrice: 50m,
            Quantity: 3);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.Quantity.Should().Be(5); // 2 + 3
        result.CartItemsCount.Should().Be(5);
    }

    [Fact]
    public async Task Handle_AddsItemToCart_WithVariant()
    {
        // Arrange
        var cart = ShoppingTestFixtures.CreateTestCustomerCart(_tenantId, _customerId, cartId: _cartId);
        _cartRepository.GetByIdAsync(_cartId, Arg.Any<CancellationToken>())
            .Returns(cart);

        var productId = Guid.NewGuid();
        var variantId = Guid.NewGuid();
        var command = new AddToCartCommand(
            CartId: _cartId,
            ProductId: productId,
            UnitPrice: 75m,
            Quantity: 1,
            VariantId: variantId);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.VariantId.Should().Be(variantId);
        result.ProductId.Should().Be(productId);
    }

    [Fact]
    public async Task Handle_AddsSameProduct_WithDifferentVariants_CreatesSeparateItems()
    {
        // Arrange
        var cart = ShoppingTestFixtures.CreateTestCustomerCart(_tenantId, _customerId, cartId: _cartId);
        var productId = Guid.NewGuid();
        var variant1 = Guid.NewGuid();
        var variant2 = Guid.NewGuid();
        
        cart.AddItem(productId, 50m, 1, variant1);
        _cartRepository.GetByIdAsync(_cartId, Arg.Any<CancellationToken>())
            .Returns(cart);

        var command = new AddToCartCommand(
            CartId: _cartId,
            ProductId: productId,
            UnitPrice: 50m,
            Quantity: 1,
            VariantId: variant2);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.CartItemsCount.Should().Be(2);
    }

    #endregion

    #region Guest Cart Tests

    [Fact]
    public async Task Handle_AddsItemToGuestCart_WithValidData()
    {
        // Arrange
        const string sessionId = "guest-session-123";
        var cart = ShoppingTestFixtures.CreateTestGuestCart(_tenantId, sessionId, cartId: _cartId);
        _cartRepository.GetByIdAsync(_cartId, Arg.Any<CancellationToken>())
            .Returns(cart);

        var productId = Guid.NewGuid();
        var command = new AddToCartCommand(
            CartId: _cartId,
            ProductId: productId,
            UnitPrice: 29.99m,
            Quantity: 1);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result.CartItemsCount.Should().Be(1);
    }

    #endregion

    #region Error Handling Tests

    [Fact]
    public async Task Handle_CartNotFound_ThrowsException()
    {
        // Arrange
        _cartRepository.GetByIdAsync(_cartId, Arg.Any<CancellationToken>())
            .Returns((Domain.Shopping.Entities.Cart?)null);

        var command = new AddToCartCommand(
            CartId: _cartId,
            ProductId: Guid.NewGuid(),
            UnitPrice: 50m,
            Quantity: 1);

        // Act & Assert
        var act = () => _handler.Handle(command, CancellationToken.None);
        await act.Should().ThrowAsync<InvalidOperationException>()
            .WithMessage("*not found*");
    }

    [Fact]
    public async Task Handle_TenantIsolation_CannotAddToAnotherTenantCart()
    {
        // Arrange
        var otherTenantId = Guid.NewGuid();
        var cart = ShoppingTestFixtures.CreateTestCustomerCart(otherTenantId, _customerId, cartId: _cartId);
        _cartRepository.GetByIdAsync(_cartId, Arg.Any<CancellationToken>())
            .Returns(cart);

        var command = new AddToCartCommand(
            CartId: _cartId,
            ProductId: Guid.NewGuid(),
            UnitPrice: 50m,
            Quantity: 1);

        // Act & Assert
        var act = () => _handler.Handle(command, CancellationToken.None);
        await act.Should().ThrowAsync<UnauthorizedAccessException>()
            .WithMessage("*Cannot access cart from another tenant*");
    }

    [Fact]
    public async Task Handle_WithInvalidProductId_ThrowsException()
    {
        // Arrange
        var cart = ShoppingTestFixtures.CreateTestCustomerCart(_tenantId, _customerId, cartId: _cartId);
        _cartRepository.GetByIdAsync(_cartId, Arg.Any<CancellationToken>())
            .Returns(cart);

        var command = new AddToCartCommand(
            CartId: _cartId,
            ProductId: Guid.Empty,
            UnitPrice: 50m,
            Quantity: 1);

        // Act & Assert
        var act = () => _handler.Handle(command, CancellationToken.None);
        await act.Should().ThrowAsync<ArgumentException>();
    }

    [Fact]
    public async Task Handle_WithNegativePrice_ThrowsException()
    {
        // Arrange
        var cart = ShoppingTestFixtures.CreateTestCustomerCart(_tenantId, _customerId, cartId: _cartId);
        _cartRepository.GetByIdAsync(_cartId, Arg.Any<CancellationToken>())
            .Returns(cart);

        var command = new AddToCartCommand(
            CartId: _cartId,
            ProductId: Guid.NewGuid(),
            UnitPrice: -10m,
            Quantity: 1);

        // Act & Assert
        var act = () => _handler.Handle(command, CancellationToken.None);
        await act.Should().ThrowAsync<ArgumentException>();
    }

    [Fact]
    public async Task Handle_WithZeroQuantity_ThrowsException()
    {
        // Arrange
        var cart = ShoppingTestFixtures.CreateTestCustomerCart(_tenantId, _customerId, cartId: _cartId);
        _cartRepository.GetByIdAsync(_cartId, Arg.Any<CancellationToken>())
            .Returns(cart);

        var command = new AddToCartCommand(
            CartId: _cartId,
            ProductId: Guid.NewGuid(),
            UnitPrice: 50m,
            Quantity: 0);

        // Act & Assert
        var act = () => _handler.Handle(command, CancellationToken.None);
        await act.Should().ThrowAsync<ArgumentException>();
    }

    [Fact]
    public async Task Handle_WithNegativeQuantity_ThrowsException()
    {
        // Arrange
        var cart = ShoppingTestFixtures.CreateTestCustomerCart(_tenantId, _customerId, cartId: _cartId);
        _cartRepository.GetByIdAsync(_cartId, Arg.Any<CancellationToken>())
            .Returns(cart);

        var command = new AddToCartCommand(
            CartId: _cartId,
            ProductId: Guid.NewGuid(),
            UnitPrice: 50m,
            Quantity: -1);

        // Act & Assert
        var act = () => _handler.Handle(command, CancellationToken.None);
        await act.Should().ThrowAsync<ArgumentException>();
    }

    #endregion

    #region Response Validation Tests

    [Fact]
    public async Task Handle_Response_ContainsCorrectLineTotal()
    {
        // Arrange
        var cart = ShoppingTestFixtures.CreateTestCustomerCart(_tenantId, _customerId, cartId: _cartId);
        _cartRepository.GetByIdAsync(_cartId, Arg.Any<CancellationToken>())
            .Returns(cart);

        var command = new AddToCartCommand(
            CartId: _cartId,
            ProductId: Guid.NewGuid(),
            UnitPrice: 25.50m,
            Quantity: 4);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.LineTotal.Should().Be(102.00m); // 25.50 * 4
    }

    [Fact]
    public async Task Handle_Response_UpdatesCartItemsCount()
    {
        // Arrange
        var cart = ShoppingTestFixtures.CreateTestCustomerCart(_tenantId, _customerId, cartId: _cartId);
        cart.AddItem(Guid.NewGuid(), 50m, 3);
        _cartRepository.GetByIdAsync(_cartId, Arg.Any<CancellationToken>())
            .Returns(cart);

        var command = new AddToCartCommand(
            CartId: _cartId,
            ProductId: Guid.NewGuid(),
            UnitPrice: 30m,
            Quantity: 2);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.CartItemsCount.Should().Be(5); // 3 + 2
    }

    [Fact]
    public async Task Handle_Response_UpdatesCartSubTotal()
    {
        // Arrange
        var cart = ShoppingTestFixtures.CreateTestCustomerCart(_tenantId, _customerId, cartId: _cartId);
        cart.AddItem(Guid.NewGuid(), 50m, 2); // 100
        _cartRepository.GetByIdAsync(_cartId, Arg.Any<CancellationToken>())
            .Returns(cart);

        var command = new AddToCartCommand(
            CartId: _cartId,
            ProductId: Guid.NewGuid(),
            UnitPrice: 25m,
            Quantity: 4); // 100

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.CartSubTotal.Should().Be(200m); // 100 + 100
    }

    [Fact]
    public async Task Handle_Response_WithLargeQuantity_Succeeds()
    {
        // Arrange
        var cart = ShoppingTestFixtures.CreateTestCustomerCart(_tenantId, _customerId, cartId: _cartId);
        _cartRepository.GetByIdAsync(_cartId, Arg.Any<CancellationToken>())
            .Returns(cart);

        var command = new AddToCartCommand(
            CartId: _cartId,
            ProductId: Guid.NewGuid(),
            UnitPrice: 10m,
            Quantity: 999);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.Quantity.Should().Be(999);
        result.CartItemsCount.Should().Be(999);
    }

    #endregion

    #region Multiple Items Tests

    [Fact]
    public async Task Handle_AddMultipleItems_ToCart()
    {
        // Arrange
        var cart = ShoppingTestFixtures.CreateTestCustomerCart(_tenantId, _customerId, cartId: _cartId);
        _cartRepository.GetByIdAsync(_cartId, Arg.Any<CancellationToken>())
            .Returns(cart);

        // Add first item
        var product1Id = Guid.NewGuid();
        var command1 = new AddToCartCommand(
            CartId: _cartId,
            ProductId: product1Id,
            UnitPrice: 50m,
            Quantity: 2);
        
        var result1 = await _handler.Handle(command1, CancellationToken.None);

        // Add second item
        var product2Id = Guid.NewGuid();
        var command2 = new AddToCartCommand(
            CartId: _cartId,
            ProductId: product2Id,
            UnitPrice: 75m,
            Quantity: 1);
        
        var result2 = await _handler.Handle(command2, CancellationToken.None);

        // Assert
        result1.CartItemsCount.Should().Be(2);
        result2.CartItemsCount.Should().Be(3); // 2 + 1
        result2.CartSubTotal.Should().Be(175m); // (50*2) + (75*1)
    }

    #endregion
}
