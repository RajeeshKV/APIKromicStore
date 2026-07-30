using FluentAssertions;
using KromicStore.Application.Common.Abstractions;
using KromicStore.Application.Features.Shopping.Commands.RemoveCartItem;
using KromicStore.Application.Features.Shopping.Abstractions;
using KromicStore.Application.Tests.Features.Shopping.Common;
using KromicStore.Infrastructure.Persistence;
using Microsoft.Extensions.Logging;
using NSubstitute;
using Xunit;

namespace KromicStore.Application.Tests.Features.Shopping.Commands.Cart;

/// <summary>
/// Handler tests for RemoveCartItemCommand.
/// Verifies removing items from cart with graceful error handling.
/// </summary>
public sealed class RemoveCartItemCommandHandlerTests
{
    private readonly ICartRepository _cartRepository;
    private readonly KromicStoreDbContext _dbContext;
    private readonly ICurrentUserService _currentUserService;
    private readonly ITenantContext _tenantContext;
    private readonly RemoveCartItemCommandHandler _handler;
    private readonly Guid _tenantId;
    private readonly Guid _customerId;
    private readonly Guid _cartId;

    public RemoveCartItemCommandHandlerTests()
    {
        _tenantId = Guid.NewGuid();
        _customerId = Guid.NewGuid();
        _cartId = Guid.NewGuid();
        _dbContext = ShoppingTestFixtures.CreateDbContext(_tenantId);
        _tenantContext = ShoppingTestFixtures.CreateTenantContext(_tenantId);
        _currentUserService = ShoppingTestFixtures.CreateCurrentUserService(_customerId);

        _cartRepository = Substitute.For<ICartRepository>();

        _handler = new RemoveCartItemCommandHandler(
            _cartRepository,
            _dbContext,
            Substitute.For<ILogger<RemoveCartItemCommandHandler>>(),
            _tenantContext,
            _currentUserService);
    }

    #region Remove Item Tests

    [Fact]
    public async Task Handle_RemovesItem_FromCart()
    {
        // Arrange
        var cart = ShoppingTestFixtures.CreateTestCustomerCart(_tenantId, _customerId, cartId: _cartId);
        var productId = Guid.NewGuid();
        cart.AddItem(productId, 50m, 2);
        _cartRepository.GetByIdAsync(_cartId, Arg.Any<CancellationToken>())
            .Returns(cart);

        var command = new RemoveCartItemCommand(
            CartId: _cartId,
            ProductId: productId);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result.CartId.Should().Be(_cartId);
        result.ProductId.Should().Be(productId);
        result.ItemFound.Should().BeTrue();
        result.CartItemsCount.Should().Be(0);
    }

    [Fact]
    public async Task Handle_RemovesItem_CallsRepository_Update()
    {
        // Arrange
        var cart = ShoppingTestFixtures.CreateTestCustomerCart(_tenantId, _customerId, cartId: _cartId);
        var productId = Guid.NewGuid();
        cart.AddItem(productId, 50m, 2);
        _cartRepository.GetByIdAsync(_cartId, Arg.Any<CancellationToken>())
            .Returns(cart);

        var command = new RemoveCartItemCommand(
            CartId: _cartId,
            ProductId: productId);

        // Act
        await _handler.Handle(command, CancellationToken.None);

        // Assert
        _cartRepository.Received(1).Update(Arg.Any<Domain.Shopping.Entities.Cart>());
    }

    [Fact]
    public async Task Handle_RemovesItem_UpdatesCartTotals()
    {
        // Arrange
        var cart = ShoppingTestFixtures.CreateTestCustomerCart(_tenantId, _customerId, cartId: _cartId);
        var product1Id = Guid.NewGuid();
        var product2Id = Guid.NewGuid();
        cart.AddItem(product1Id, 50m, 2); // 100
        cart.AddItem(product2Id, 30m, 1); // 30
        _cartRepository.GetByIdAsync(_cartId, Arg.Any<CancellationToken>())
            .Returns(cart);

        var command = new RemoveCartItemCommand(
            CartId: _cartId,
            ProductId: product1Id);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.CartItemsCount.Should().Be(1);
        result.CartSubTotal.Should().Be(30m);
    }

    #endregion

    #region Non-Existent Item Tests

    [Fact]
    public async Task Handle_RemovesNonExistentItem_HandlesGracefully()
    {
        // Arrange
        var cart = ShoppingTestFixtures.CreateTestCustomerCart(_tenantId, _customerId, cartId: _cartId);
        _cartRepository.GetByIdAsync(_cartId, Arg.Any<CancellationToken>())
            .Returns(cart);

        var command = new RemoveCartItemCommand(
            CartId: _cartId,
            ProductId: Guid.NewGuid());

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.ItemFound.Should().BeFalse();
        result.CartItemsCount.Should().Be(0);
    }

    [Fact]
    public async Task Handle_RemoveNonExistentItem_DoesNotThrow()
    {
        // Arrange
        var cart = ShoppingTestFixtures.CreateTestCustomerCart(_tenantId, _customerId, cartId: _cartId);
        var product1Id = Guid.NewGuid();
        cart.AddItem(product1Id, 50m, 2);
        _cartRepository.GetByIdAsync(_cartId, Arg.Any<CancellationToken>())
            .Returns(cart);

        var command = new RemoveCartItemCommand(
            CartId: _cartId,
            ProductId: Guid.NewGuid()); // Different product

        // Act & Assert
        var act = () => _handler.Handle(command, CancellationToken.None);
        await act.Should().NotThrowAsync();
    }

    [Fact]
    public async Task Handle_RemoveNonExistentItem_StillCallsRepository_Update()
    {
        // Arrange
        var cart = ShoppingTestFixtures.CreateTestCustomerCart(_tenantId, _customerId, cartId: _cartId);
        _cartRepository.GetByIdAsync(_cartId, Arg.Any<CancellationToken>())
            .Returns(cart);

        var command = new RemoveCartItemCommand(
            CartId: _cartId,
            ProductId: Guid.NewGuid());

        // Act
        await _handler.Handle(command, CancellationToken.None);

        // Assert
        _cartRepository.Received(1).Update(Arg.Any<Domain.Shopping.Entities.Cart>());
    }

    #endregion

    #region Variant Tests

    [Fact]
    public async Task Handle_RemovesItem_WithVariant()
    {
        // Arrange
        var cart = ShoppingTestFixtures.CreateTestCustomerCart(_tenantId, _customerId, cartId: _cartId);
        var productId = Guid.NewGuid();
        var variantId = Guid.NewGuid();
        cart.AddItem(productId, 75m, 2, variantId);
        _cartRepository.GetByIdAsync(_cartId, Arg.Any<CancellationToken>())
            .Returns(cart);

        var command = new RemoveCartItemCommand(
            CartId: _cartId,
            ProductId: productId,
            VariantId: variantId);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.ItemFound.Should().BeTrue();
        result.VariantId.Should().Be(variantId);
        result.CartItemsCount.Should().Be(0);
    }

    [Fact]
    public async Task Handle_RemovesVariant_OnlyRemovesSpecificVariant()
    {
        // Arrange
        var cart = ShoppingTestFixtures.CreateTestCustomerCart(_tenantId, _customerId, cartId: _cartId);
        var productId = Guid.NewGuid();
        var variant1 = Guid.NewGuid();
        var variant2 = Guid.NewGuid();
        cart.AddItem(productId, 50m, 1, variant1);
        cart.AddItem(productId, 50m, 1, variant2);
        _cartRepository.GetByIdAsync(_cartId, Arg.Any<CancellationToken>())
            .Returns(cart);

        var command = new RemoveCartItemCommand(
            CartId: _cartId,
            ProductId: productId,
            VariantId: variant1);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.ItemFound.Should().BeTrue();
        result.CartItemsCount.Should().Be(1); // variant2 still exists
    }

    #endregion

    #region Error Handling Tests

    [Fact]
    public async Task Handle_CartNotFound_ThrowsException()
    {
        // Arrange
        _cartRepository.GetByIdAsync(_cartId, Arg.Any<CancellationToken>())
            .Returns((Domain.Shopping.Entities.Cart?)null);

        var command = new RemoveCartItemCommand(
            CartId: _cartId,
            ProductId: Guid.NewGuid());

        // Act & Assert
        var act = () => _handler.Handle(command, CancellationToken.None);
        await act.Should().ThrowAsync<InvalidOperationException>()
            .WithMessage("*not found*");
    }

    [Fact]
    public async Task Handle_TenantIsolation_CannotRemoveFromAnotherTenantCart()
    {
        // Arrange
        var otherTenantId = Guid.NewGuid();
        var cart = ShoppingTestFixtures.CreateTestCustomerCart(otherTenantId, _customerId, cartId: _cartId);
        var productId = Guid.NewGuid();
        cart.AddItem(productId, 50m, 2);
        _cartRepository.GetByIdAsync(_cartId, Arg.Any<CancellationToken>())
            .Returns(cart);

        var command = new RemoveCartItemCommand(
            CartId: _cartId,
            ProductId: productId);

        // Act & Assert
        var act = () => _handler.Handle(command, CancellationToken.None);
        await act.Should().ThrowAsync<UnauthorizedAccessException>()
            .WithMessage("*Cannot access cart from another tenant*");
    }

    #endregion

    #region Multiple Items Tests

    [Fact]
    public async Task Handle_RemoveFromMultipleItems_RemainingItemsStayInCart()
    {
        // Arrange
        var cart = ShoppingTestFixtures.CreateTestCustomerCart(_tenantId, _customerId, cartId: _cartId);
        var product1Id = Guid.NewGuid();
        var product2Id = Guid.NewGuid();
        var product3Id = Guid.NewGuid();
        cart.AddItem(product1Id, 50m, 1);
        cart.AddItem(product2Id, 30m, 1);
        cart.AddItem(product3Id, 20m, 1);
        _cartRepository.GetByIdAsync(_cartId, Arg.Any<CancellationToken>())
            .Returns(cart);

        var command = new RemoveCartItemCommand(
            CartId: _cartId,
            ProductId: product2Id);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.CartItemsCount.Should().Be(2);
        result.CartSubTotal.Should().Be(70m); // 50 + 20
    }

    [Fact]
    public async Task Handle_RemoveAllItems_OneByOne()
    {
        // Arrange
        var cart = ShoppingTestFixtures.CreateTestCustomerCart(_tenantId, _customerId, cartId: _cartId);
        var product1Id = Guid.NewGuid();
        var product2Id = Guid.NewGuid();
        cart.AddItem(product1Id, 50m, 1);
        cart.AddItem(product2Id, 30m, 1);
        _cartRepository.GetByIdAsync(_cartId, Arg.Any<CancellationToken>())
            .Returns(cart);

        // Remove first item
        var command1 = new RemoveCartItemCommand(
            CartId: _cartId,
            ProductId: product1Id);
        var result1 = await _handler.Handle(command1, CancellationToken.None);

        // Remove second item
        var command2 = new RemoveCartItemCommand(
            CartId: _cartId,
            ProductId: product2Id);
        var result2 = await _handler.Handle(command2, CancellationToken.None);

        // Assert
        result1.CartItemsCount.Should().Be(1);
        result2.CartItemsCount.Should().Be(0);
        result2.CartSubTotal.Should().Be(0m);
    }

    #endregion

    #region Response Validation Tests

    [Fact]
    public async Task Handle_Response_ContainsCorrectData()
    {
        // Arrange
        var cart = ShoppingTestFixtures.CreateTestCustomerCart(_tenantId, _customerId, cartId: _cartId);
        var productId = Guid.NewGuid();
        cart.AddItem(productId, 50m, 2);
        cart.AddItem(Guid.NewGuid(), 30m, 1);
        _cartRepository.GetByIdAsync(_cartId, Arg.Any<CancellationToken>())
            .Returns(cart);

        var command = new RemoveCartItemCommand(
            CartId: _cartId,
            ProductId: productId);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.CartId.Should().Be(_cartId);
        result.ProductId.Should().Be(productId);
        result.ItemFound.Should().BeTrue();
        result.CartItemsCount.Should().Be(1);
        result.CartSubTotal.Should().Be(30m);
    }

    [Fact]
    public async Task Handle_Response_ContainsCorrectData_WhenItemNotFound()
    {
        // Arrange
        var cart = ShoppingTestFixtures.CreateTestCustomerCart(_tenantId, _customerId, cartId: _cartId);
        var existingProductId = Guid.NewGuid();
        cart.AddItem(existingProductId, 50m, 1);
        _cartRepository.GetByIdAsync(_cartId, Arg.Any<CancellationToken>())
            .Returns(cart);

        var nonExistentProductId = Guid.NewGuid();
        var command = new RemoveCartItemCommand(
            CartId: _cartId,
            ProductId: nonExistentProductId);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.CartId.Should().Be(_cartId);
        result.ProductId.Should().Be(nonExistentProductId);
        result.ItemFound.Should().BeFalse();
        result.CartItemsCount.Should().Be(1);
        result.CartSubTotal.Should().Be(50m);
    }

    #endregion
}
