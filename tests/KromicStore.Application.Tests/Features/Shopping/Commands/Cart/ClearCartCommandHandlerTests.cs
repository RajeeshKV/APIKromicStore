using FluentAssertions;
using KromicStore.Application.Common.Abstractions;
using KromicStore.Application.Features.Shopping.Commands.ClearCart;
using KromicStore.Application.Features.Shopping.Abstractions;
using KromicStore.Application.Tests.Features.Shopping.Common;
using KromicStore.Infrastructure.Persistence;
using Microsoft.Extensions.Logging;
using NSubstitute;
using Xunit;

namespace KromicStore.Application.Tests.Features.Shopping.Commands.Cart;

/// <summary>
/// Handler tests for ClearCartCommand.
/// Verifies clearing all items from a cart.
/// </summary>
public sealed class ClearCartCommandHandlerTests
{
    private readonly ICartRepository _cartRepository;
    private readonly KromicStoreDbContext _dbContext;
    private readonly ICurrentUserService _currentUserService;
    private readonly ITenantContext _tenantContext;
    private readonly ClearCartCommandHandler _handler;
    private readonly Guid _tenantId;
    private readonly Guid _customerId;
    private readonly Guid _cartId;

    public ClearCartCommandHandlerTests()
    {
        _tenantId = Guid.NewGuid();
        _customerId = Guid.NewGuid();
        _cartId = Guid.NewGuid();
        _dbContext = ShoppingTestFixtures.CreateDbContext(_tenantId);
        _tenantContext = ShoppingTestFixtures.CreateTenantContext(_tenantId);
        _currentUserService = ShoppingTestFixtures.CreateCurrentUserService(_customerId);

        _cartRepository = Substitute.For<ICartRepository>();

        _handler = new ClearCartCommandHandler(
            _cartRepository,
            _dbContext,
            Substitute.For<ILogger<ClearCartCommandHandler>>(),
            _tenantContext,
            _currentUserService);
    }

    #region Clear Cart Tests

    [Fact]
    public async Task Handle_ClearsCart_WithItems()
    {
        // Arrange
        var cart = ShoppingTestFixtures.CreateTestCustomerCart(_tenantId, _customerId, cartId: _cartId);
        cart.AddItem(Guid.NewGuid(), 50m, 2);
        cart.AddItem(Guid.NewGuid(), 75m, 3);
        _cartRepository.GetByIdAsync(_cartId, Arg.Any<CancellationToken>())
            .Returns(cart);

        var command = new ClearCartCommand(CartId: _cartId);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result.CartId.Should().Be(_cartId);
        result.PreviousItemsCount.Should().Be(5);
        result.PreviousSubTotal.Should().Be(325m); // (50*2) + (75*3)
        result.CartNowEmpty.Should().BeTrue();
    }

    [Fact]
    public async Task Handle_ClearsCart_CallsRepository_Update()
    {
        // Arrange
        var cart = ShoppingTestFixtures.CreateTestCustomerCart(_tenantId, _customerId, cartId: _cartId);
        cart.AddItem(Guid.NewGuid(), 50m, 2);
        _cartRepository.GetByIdAsync(_cartId, Arg.Any<CancellationToken>())
            .Returns(cart);

        var command = new ClearCartCommand(CartId: _cartId);

        // Act
        await _handler.Handle(command, CancellationToken.None);

        // Assert
        _cartRepository.Received(1).Update(Arg.Any<Domain.Shopping.Entities.Cart>());
    }

    [Fact]
    public async Task Handle_ClearsCart_RemovesAllItems()
    {
        // Arrange
        var cart = ShoppingTestFixtures.CreateTestCustomerCart(_tenantId, _customerId, cartId: _cartId);
        cart.AddItem(Guid.NewGuid(), 50m, 2);
        cart.AddItem(Guid.NewGuid(), 75m, 3);
        cart.AddItem(Guid.NewGuid(), 25m, 1);
        _cartRepository.GetByIdAsync(_cartId, Arg.Any<CancellationToken>())
            .Returns(cart);

        var command = new ClearCartCommand(CartId: _cartId);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.PreviousItemsCount.Should().Be(6); // 2 + 3 + 1
        result.CartNowEmpty.Should().BeTrue();
    }

    #endregion

    #region Empty Cart Tests

    [Fact]
    public async Task Handle_ClearsEmptyCart_RemainsEmpty()
    {
        // Arrange
        var cart = ShoppingTestFixtures.CreateTestCustomerCart(_tenantId, _customerId, cartId: _cartId);
        _cartRepository.GetByIdAsync(_cartId, Arg.Any<CancellationToken>())
            .Returns(cart);

        var command = new ClearCartCommand(CartId: _cartId);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.PreviousItemsCount.Should().Be(0);
        result.PreviousSubTotal.Should().Be(0m);
        result.CartNowEmpty.Should().BeTrue();
    }

    [Fact]
    public async Task Handle_ClearEmptyCart_CallsRepository_Update()
    {
        // Arrange
        var cart = ShoppingTestFixtures.CreateTestCustomerCart(_tenantId, _customerId, cartId: _cartId);
        _cartRepository.GetByIdAsync(_cartId, Arg.Any<CancellationToken>())
            .Returns(cart);

        var command = new ClearCartCommand(CartId: _cartId);

        // Act
        await _handler.Handle(command, CancellationToken.None);

        // Assert
        _cartRepository.Received(1).Update(Arg.Any<Domain.Shopping.Entities.Cart>());
    }

    #endregion

    #region Variant Tests

    [Fact]
    public async Task Handle_ClearsCart_WithVariants()
    {
        // Arrange
        var cart = ShoppingTestFixtures.CreateTestCustomerCart(_tenantId, _customerId, cartId: _cartId);
        var productId = Guid.NewGuid();
        var variant1 = Guid.NewGuid();
        var variant2 = Guid.NewGuid();
        cart.AddItem(productId, 50m, 1, variant1);
        cart.AddItem(productId, 50m, 1, variant2);
        cart.AddItem(Guid.NewGuid(), 75m, 2);
        _cartRepository.GetByIdAsync(_cartId, Arg.Any<CancellationToken>())
            .Returns(cart);

        var command = new ClearCartCommand(CartId: _cartId);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.PreviousItemsCount.Should().Be(4); // 1 + 1 + 2
        result.CartNowEmpty.Should().BeTrue();
    }

    #endregion

    #region Error Handling Tests

    [Fact]
    public async Task Handle_CartNotFound_ThrowsException()
    {
        // Arrange
        _cartRepository.GetByIdAsync(_cartId, Arg.Any<CancellationToken>())
            .Returns((Domain.Shopping.Entities.Cart?)null);

        var command = new ClearCartCommand(CartId: _cartId);

        // Act & Assert
        var act = () => _handler.Handle(command, CancellationToken.None);
        await act.Should().ThrowAsync<InvalidOperationException>()
            .WithMessage("*not found*");
    }

    [Fact]
    public async Task Handle_TenantIsolation_CannotClearAnotherTenantCart()
    {
        // Arrange
        var otherTenantId = Guid.NewGuid();
        var cart = ShoppingTestFixtures.CreateTestCustomerCart(otherTenantId, _customerId, cartId: _cartId);
        cart.AddItem(Guid.NewGuid(), 50m, 2);
        _cartRepository.GetByIdAsync(_cartId, Arg.Any<CancellationToken>())
            .Returns(cart);

        var command = new ClearCartCommand(CartId: _cartId);

        // Act & Assert
        var act = () => _handler.Handle(command, CancellationToken.None);
        await act.Should().ThrowAsync<UnauthorizedAccessException>()
            .WithMessage("*Cannot access cart from another tenant*");
    }

    [Fact]
    public async Task Handle_WithNullTenantContext_ThrowsException()
    {
        // Arrange
        var tenantContext = Substitute.For<ITenantContext>();
        tenantContext.TenantId.Returns((Guid?)null);

        var cart = ShoppingTestFixtures.CreateTestCustomerCart(_tenantId, _customerId, cartId: _cartId);
        
        var cartRepository = Substitute.For<ICartRepository>();
        cartRepository.GetByIdAsync(_cartId, Arg.Any<CancellationToken>())
            .Returns(cart);

        var handler = new ClearCartCommandHandler(
            cartRepository,
            _dbContext,
            Substitute.For<ILogger<ClearCartCommandHandler>>(),
            tenantContext,
            _currentUserService);

        var command = new ClearCartCommand(CartId: _cartId);

        // Act & Assert
        var act = () => handler.Handle(command, CancellationToken.None);
        await act.Should().ThrowAsync<InvalidOperationException>()
            .WithMessage("*Tenant context is not resolved*");
    }

    #endregion

    #region Response Validation Tests

    [Fact]
    public async Task Handle_Response_ContainsCorrectData()
    {
        // Arrange
        var cart = ShoppingTestFixtures.CreateTestCustomerCart(_tenantId, _customerId, cartId: _cartId);
        var product1Id = Guid.NewGuid();
        var product2Id = Guid.NewGuid();
        cart.AddItem(product1Id, 50m, 2);
        cart.AddItem(product2Id, 75m, 3);
        _cartRepository.GetByIdAsync(_cartId, Arg.Any<CancellationToken>())
            .Returns(cart);

        var command = new ClearCartCommand(CartId: _cartId);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.CartId.Should().Be(_cartId);
        result.PreviousItemsCount.Should().Be(5);
        result.PreviousSubTotal.Should().Be(325m); // (50*2) + (75*3)
        result.CartNowEmpty.Should().BeTrue();
    }

    [Fact]
    public async Task Handle_Response_WithEmptyCart_ContainsZeros()
    {
        // Arrange
        var cart = ShoppingTestFixtures.CreateTestCustomerCart(_tenantId, _customerId, cartId: _cartId);
        _cartRepository.GetByIdAsync(_cartId, Arg.Any<CancellationToken>())
            .Returns(cart);

        var command = new ClearCartCommand(CartId: _cartId);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.CartId.Should().Be(_cartId);
        result.PreviousItemsCount.Should().Be(0);
        result.PreviousSubTotal.Should().Be(0m);
        result.CartNowEmpty.Should().BeTrue();
    }

    #endregion

    #region Guest Cart Tests

    [Fact]
    public async Task Handle_ClearsGuestCart_WithItems()
    {
        // Arrange
        const string sessionId = "guest-session-123";
        var cart = ShoppingTestFixtures.CreateTestGuestCart(_tenantId, sessionId, cartId: _cartId);
        cart.AddItem(Guid.NewGuid(), 50m, 2);
        cart.AddItem(Guid.NewGuid(), 75m, 3);
        _cartRepository.GetByIdAsync(_cartId, Arg.Any<CancellationToken>())
            .Returns(cart);

        var command = new ClearCartCommand(CartId: _cartId);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.PreviousItemsCount.Should().Be(5);
        result.CartNowEmpty.Should().BeTrue();
    }

    #endregion

    #region Reclear Tests

    [Fact]
    public async Task Handle_ReClearCart_AfterClear_RemainsEmpty()
    {
        // Arrange
        var cart = ShoppingTestFixtures.CreateTestCustomerCart(_tenantId, _customerId, cartId: _cartId);
        cart.AddItem(Guid.NewGuid(), 50m, 2);
        _cartRepository.GetByIdAsync(_cartId, Arg.Any<CancellationToken>())
            .Returns(cart);

        var command = new ClearCartCommand(CartId: _cartId);

        // Act
        var result1 = await _handler.Handle(command, CancellationToken.None);
        var result2 = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result1.PreviousItemsCount.Should().Be(2);
        result2.PreviousItemsCount.Should().Be(0);
        result2.CartNowEmpty.Should().BeTrue();
    }

    #endregion
}
