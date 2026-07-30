using FluentAssertions;
using KromicStore.Application.Common.Abstractions;
using KromicStore.Application.Features.Shopping.Commands.UpdateCartItem;
using KromicStore.Application.Features.Shopping.Abstractions;
using KromicStore.Application.Tests.Features.Shopping.Common;
using KromicStore.Infrastructure.Persistence;
using Microsoft.Extensions.Logging;
using NSubstitute;
using Xunit;

namespace KromicStore.Application.Tests.Features.Shopping.Commands.Cart;

/// <summary>
/// Handler tests for UpdateCartItemCommand.
/// Verifies updating item quantities and removing items.
/// </summary>
public sealed class UpdateCartItemCommandHandlerTests
{
    private readonly ICartRepository _cartRepository;
    private readonly KromicStoreDbContext _dbContext;
    private readonly ICurrentUserService _currentUserService;
    private readonly ITenantContext _tenantContext;
    private readonly UpdateCartItemCommandHandler _handler;
    private readonly Guid _tenantId;
    private readonly Guid _customerId;
    private readonly Guid _cartId;

    public UpdateCartItemCommandHandlerTests()
    {
        _tenantId = Guid.NewGuid();
        _customerId = Guid.NewGuid();
        _cartId = Guid.NewGuid();
        _dbContext = ShoppingTestFixtures.CreateDbContext(_tenantId);
        _tenantContext = ShoppingTestFixtures.CreateTenantContext(_tenantId);
        _currentUserService = ShoppingTestFixtures.CreateCurrentUserService(_customerId);

        _cartRepository = Substitute.For<ICartRepository>();

        _handler = new UpdateCartItemCommandHandler(
            _cartRepository,
            _dbContext,
            Substitute.For<ILogger<UpdateCartItemCommandHandler>>(),
            _tenantContext,
            _currentUserService);
    }

    #region Update Quantity Tests

    [Fact]
    public async Task Handle_UpdatesQuantity_WithValidData()
    {
        // Arrange
        var cart = ShoppingTestFixtures.CreateTestCustomerCart(_tenantId, _customerId, cartId: _cartId);
        var productId = Guid.NewGuid();
        cart.AddItem(productId, 50m, 2);
        _cartRepository.GetByIdAsync(_cartId, Arg.Any<CancellationToken>())
            .Returns(cart);

        var command = new UpdateCartItemCommand(
            CartId: _cartId,
            ProductId: productId,
            NewQuantity: 5);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result.CartId.Should().Be(_cartId);
        result.ProductId.Should().Be(productId);
        result.Quantity.Should().Be(5);
        result.ItemRemoved.Should().BeFalse();
    }

    [Fact]
    public async Task Handle_UpdateQuantity_CallsRepository_Update()
    {
        // Arrange
        var cart = ShoppingTestFixtures.CreateTestCustomerCart(_tenantId, _customerId, cartId: _cartId);
        var productId = Guid.NewGuid();
        cart.AddItem(productId, 50m, 2);
        _cartRepository.GetByIdAsync(_cartId, Arg.Any<CancellationToken>())
            .Returns(cart);

        var command = new UpdateCartItemCommand(
            CartId: _cartId,
            ProductId: productId,
            NewQuantity: 3);

        // Act
        await _handler.Handle(command, CancellationToken.None);

        // Assert
        _cartRepository.Received(1).Update(Arg.Any<Domain.Shopping.Entities.Cart>());
    }

    [Fact]
    public async Task Handle_UpdatesQuantity_CalculatesCorrectLineTotal()
    {
        // Arrange
        var cart = ShoppingTestFixtures.CreateTestCustomerCart(_tenantId, _customerId, cartId: _cartId);
        var productId = Guid.NewGuid();
        cart.AddItem(productId, 25m, 2);
        _cartRepository.GetByIdAsync(_cartId, Arg.Any<CancellationToken>())
            .Returns(cart);

        var command = new UpdateCartItemCommand(
            CartId: _cartId,
            ProductId: productId,
            NewQuantity: 4);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.LineTotal.Should().Be(100m); // 25 * 4
        result.UnitPrice.Should().Be(25m);
    }

    [Fact]
    public async Task Handle_UpdatesQuantity_UpdatesCartTotals()
    {
        // Arrange
        var cart = ShoppingTestFixtures.CreateTestCustomerCart(_tenantId, _customerId, cartId: _cartId);
        var product1Id = Guid.NewGuid();
        var product2Id = Guid.NewGuid();
        cart.AddItem(product1Id, 50m, 2); // 100
        cart.AddItem(product2Id, 30m, 1); // 30
        _cartRepository.GetByIdAsync(_cartId, Arg.Any<CancellationToken>())
            .Returns(cart);

        var command = new UpdateCartItemCommand(
            CartId: _cartId,
            ProductId: product1Id,
            NewQuantity: 3);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.CartItemsCount.Should().Be(4); // 3 + 1
        result.CartSubTotal.Should().Be(180m); // (50*3) + (30*1)
    }

    [Fact]
    public async Task Handle_UpdateQuantity_ToOne_Succeeds()
    {
        // Arrange
        var cart = ShoppingTestFixtures.CreateTestCustomerCart(_tenantId, _customerId, cartId: _cartId);
        var productId = Guid.NewGuid();
        cart.AddItem(productId, 50m, 5);
        _cartRepository.GetByIdAsync(_cartId, Arg.Any<CancellationToken>())
            .Returns(cart);

        var command = new UpdateCartItemCommand(
            CartId: _cartId,
            ProductId: productId,
            NewQuantity: 1);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.Quantity.Should().Be(1);
        result.ItemRemoved.Should().BeFalse();
    }

    #endregion

    #region Remove Item Tests

    [Fact]
    public async Task Handle_SetQuantityToZero_RemovesItem()
    {
        // Arrange
        var cart = ShoppingTestFixtures.CreateTestCustomerCart(_tenantId, _customerId, cartId: _cartId);
        var productId = Guid.NewGuid();
        cart.AddItem(productId, 50m, 2);
        _cartRepository.GetByIdAsync(_cartId, Arg.Any<CancellationToken>())
            .Returns(cart);

        var command = new UpdateCartItemCommand(
            CartId: _cartId,
            ProductId: productId,
            NewQuantity: 0);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.ItemRemoved.Should().BeTrue();
        result.CartItemsCount.Should().Be(0);
    }

    [Fact]
    public async Task Handle_RemoveItem_UpdatesCartTotals()
    {
        // Arrange
        var cart = ShoppingTestFixtures.CreateTestCustomerCart(_tenantId, _customerId, cartId: _cartId);
        var product1Id = Guid.NewGuid();
        var product2Id = Guid.NewGuid();
        cart.AddItem(product1Id, 50m, 2); // 100
        cart.AddItem(product2Id, 30m, 1); // 30
        _cartRepository.GetByIdAsync(_cartId, Arg.Any<CancellationToken>())
            .Returns(cart);

        var command = new UpdateCartItemCommand(
            CartId: _cartId,
            ProductId: product1Id,
            NewQuantity: 0);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.ItemRemoved.Should().BeTrue();
        result.CartItemsCount.Should().Be(1);
        result.CartSubTotal.Should().Be(30m);
    }

    [Fact]
    public async Task Handle_RemoveItem_CallsRepository_Update()
    {
        // Arrange
        var cart = ShoppingTestFixtures.CreateTestCustomerCart(_tenantId, _customerId, cartId: _cartId);
        var productId = Guid.NewGuid();
        cart.AddItem(productId, 50m, 2);
        _cartRepository.GetByIdAsync(_cartId, Arg.Any<CancellationToken>())
            .Returns(cart);

        var command = new UpdateCartItemCommand(
            CartId: _cartId,
            ProductId: productId,
            NewQuantity: 0);

        // Act
        await _handler.Handle(command, CancellationToken.None);

        // Assert
        _cartRepository.Received(1).Update(Arg.Any<Domain.Shopping.Entities.Cart>());
    }

    #endregion

    #region Variant Tests

    [Fact]
    public async Task Handle_UpdateQuantity_WithVariant()
    {
        // Arrange
        var cart = ShoppingTestFixtures.CreateTestCustomerCart(_tenantId, _customerId, cartId: _cartId);
        var productId = Guid.NewGuid();
        var variantId = Guid.NewGuid();
        cart.AddItem(productId, 75m, 2, variantId);
        _cartRepository.GetByIdAsync(_cartId, Arg.Any<CancellationToken>())
            .Returns(cart);

        var command = new UpdateCartItemCommand(
            CartId: _cartId,
            ProductId: productId,
            NewQuantity: 3,
            VariantId: variantId);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.VariantId.Should().Be(variantId);
        result.Quantity.Should().Be(3);
    }

    [Fact]
    public async Task Handle_RemoveVariant_OnlyRemovesSpecificVariant()
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

        var command = new UpdateCartItemCommand(
            CartId: _cartId,
            ProductId: productId,
            NewQuantity: 0,
            VariantId: variant1);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.ItemRemoved.Should().BeTrue();
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

        var command = new UpdateCartItemCommand(
            CartId: _cartId,
            ProductId: Guid.NewGuid(),
            NewQuantity: 2);

        // Act & Assert
        var act = () => _handler.Handle(command, CancellationToken.None);
        await act.Should().ThrowAsync<InvalidOperationException>()
            .WithMessage("*not found*");
    }

    [Fact]
    public async Task Handle_ItemNotFound_ThrowsException()
    {
        // Arrange
        var cart = ShoppingTestFixtures.CreateTestCustomerCart(_tenantId, _customerId, cartId: _cartId);
        _cartRepository.GetByIdAsync(_cartId, Arg.Any<CancellationToken>())
            .Returns(cart);

        var command = new UpdateCartItemCommand(
            CartId: _cartId,
            ProductId: Guid.NewGuid(),
            NewQuantity: 2);

        // Act & Assert
        var act = () => _handler.Handle(command, CancellationToken.None);
        await act.Should().ThrowAsync<InvalidOperationException>()
            .WithMessage("*Item not found in cart*");
    }

    [Fact]
    public async Task Handle_TenantIsolation_CannotUpdateAnotherTenantCart()
    {
        // Arrange
        var otherTenantId = Guid.NewGuid();
        var cart = ShoppingTestFixtures.CreateTestCustomerCart(otherTenantId, _customerId, cartId: _cartId);
        var productId = Guid.NewGuid();
        cart.AddItem(productId, 50m, 2);
        _cartRepository.GetByIdAsync(_cartId, Arg.Any<CancellationToken>())
            .Returns(cart);

        var command = new UpdateCartItemCommand(
            CartId: _cartId,
            ProductId: productId,
            NewQuantity: 3);

        // Act & Assert
        var act = () => _handler.Handle(command, CancellationToken.None);
        await act.Should().ThrowAsync<UnauthorizedAccessException>()
            .WithMessage("*Cannot access cart from another tenant*");
    }

    [Fact]
    public async Task Handle_WithNegativeQuantity_ThrowsException()
    {
        // Arrange
        var cart = ShoppingTestFixtures.CreateTestCustomerCart(_tenantId, _customerId, cartId: _cartId);
        var productId = Guid.NewGuid();
        cart.AddItem(productId, 50m, 2);
        _cartRepository.GetByIdAsync(_cartId, Arg.Any<CancellationToken>())
            .Returns(cart);

        var command = new UpdateCartItemCommand(
            CartId: _cartId,
            ProductId: productId,
            NewQuantity: -1);

        // Act & Assert
        var act = () => _handler.Handle(command, CancellationToken.None);
        await act.Should().ThrowAsync<ArgumentException>();
    }

    [Fact]
    public async Task Handle_WithLargeQuantity_Succeeds()
    {
        // Arrange
        var cart = ShoppingTestFixtures.CreateTestCustomerCart(_tenantId, _customerId, cartId: _cartId);
        var productId = Guid.NewGuid();
        cart.AddItem(productId, 50m, 1);
        _cartRepository.GetByIdAsync(_cartId, Arg.Any<CancellationToken>())
            .Returns(cart);

        var command = new UpdateCartItemCommand(
            CartId: _cartId,
            ProductId: productId,
            NewQuantity: 999);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.Quantity.Should().Be(999);
        result.ItemRemoved.Should().BeFalse();
    }

    #endregion

    #region Response Validation Tests

    [Fact]
    public async Task Handle_Response_ContainsCorrectData_OnUpdate()
    {
        // Arrange
        var cart = ShoppingTestFixtures.CreateTestCustomerCart(_tenantId, _customerId, cartId: _cartId);
        var productId = Guid.NewGuid();
        cart.AddItem(productId, 35m, 2);
        _cartRepository.GetByIdAsync(_cartId, Arg.Any<CancellationToken>())
            .Returns(cart);

        var command = new UpdateCartItemCommand(
            CartId: _cartId,
            ProductId: productId,
            NewQuantity: 4);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.CartId.Should().Be(_cartId);
        result.ProductId.Should().Be(productId);
        result.Quantity.Should().Be(4);
        result.UnitPrice.Should().Be(35m);
        result.LineTotal.Should().Be(140m); // 35 * 4
        result.ItemRemoved.Should().BeFalse();
    }

    [Fact]
    public async Task Handle_Response_ContainsCorrectData_OnRemoval()
    {
        // Arrange
        var cart = ShoppingTestFixtures.CreateTestCustomerCart(_tenantId, _customerId, cartId: _cartId);
        var productId = Guid.NewGuid();
        cart.AddItem(productId, 50m, 3);
        _cartRepository.GetByIdAsync(_cartId, Arg.Any<CancellationToken>())
            .Returns(cart);

        var command = new UpdateCartItemCommand(
            CartId: _cartId,
            ProductId: productId,
            NewQuantity: 0);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.CartId.Should().Be(_cartId);
        result.ProductId.Should().Be(productId);
        result.ItemRemoved.Should().BeTrue();
        result.CartItemsCount.Should().Be(0);
    }

    #endregion
}
