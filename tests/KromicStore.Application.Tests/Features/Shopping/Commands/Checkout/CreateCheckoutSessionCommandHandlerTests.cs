using FluentAssertions;
using KromicStore.Application.Common.Abstractions;
using KromicStore.Application.Features.Shopping.Commands.CreateCheckoutSession;
using KromicStore.Application.Features.Shopping.Abstractions;
using KromicStore.Application.Tests.Features.Shopping.Common;
using KromicStore.Infrastructure.Persistence;
using Microsoft.Extensions.Logging;
using NSubstitute;
using Xunit;

namespace KromicStore.Application.Tests.Features.Shopping.Commands.Checkout;

/// <summary>
/// Handler tests for CreateCheckoutSessionCommand.
/// Verifies creating checkout sessions from shopping carts.
/// </summary>
public sealed class CreateCheckoutSessionCommandHandlerTests
{
    private readonly ICartRepository _cartRepository;
    private readonly ICheckoutSessionRepository _checkoutSessionRepository;
    private readonly KromicStoreDbContext _dbContext;
    private readonly ITenantContext _tenantContext;
    private readonly CreateCheckoutSessionCommandHandler _handler;
    private readonly Guid _tenantId;
    private readonly Guid _customerId;
    private readonly Guid _cartId;

    public CreateCheckoutSessionCommandHandlerTests()
    {
        _tenantId = Guid.NewGuid();
        _customerId = Guid.NewGuid();
        _cartId = Guid.NewGuid();
        _dbContext = ShoppingTestFixtures.CreateDbContext(_tenantId);
        _tenantContext = ShoppingTestFixtures.CreateTenantContext(_tenantId);

        _cartRepository = Substitute.For<ICartRepository>();
        _checkoutSessionRepository = Substitute.For<ICheckoutSessionRepository>();

        _handler = new CreateCheckoutSessionCommandHandler(
            _cartRepository,
            _checkoutSessionRepository,
            _dbContext,
            Substitute.For<ILogger<CreateCheckoutSessionCommandHandler>>(),
            _tenantContext);
    }

    #region Create Checkout Session Tests

    [Fact]
    public async Task Handle_CreatesCheckoutSession_WithValidCart()
    {
        // Arrange
        var cart = ShoppingTestFixtures.CreateTestCustomerCart(_tenantId, _customerId, cartId: _cartId);
        var productId = Guid.NewGuid();
        cart.AddItem(productId, 50m, 2);
        
        _cartRepository.GetByIdAsync(_cartId, Arg.Any<CancellationToken>())
            .Returns(cart);

        var command = new CreateCheckoutSessionCommand(CartId: _cartId, CustomerId: _customerId);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result.CheckoutSessionId.Should().NotBe(Guid.Empty);
        result.CustomerId.Should().Be(_customerId);
        result.ItemsCount.Should().Be(1);
        result.Items.Should().HaveCount(1);
        result.Items[0].ProductId.Should().Be(productId);
        result.Items[0].Quantity.Should().Be(2);
        result.Items[0].UnitPrice.Should().Be(50m);
        result.Items[0].LineTotal.Should().Be(100m);
        result.SubTotal.Should().Be(100m);
        result.Status.Should().Be("Draft");
    }

    [Fact]
    public async Task Handle_CreatesCheckoutSession_WithMultipleItems()
    {
        // Arrange
        var cart = ShoppingTestFixtures.CreateTestCustomerCart(_tenantId, _customerId, cartId: _cartId);
        var product1Id = Guid.NewGuid();
        var product2Id = Guid.NewGuid();
        cart.AddItem(product1Id, 50m, 2);
        cart.AddItem(product2Id, 75m, 1);
        
        _cartRepository.GetByIdAsync(_cartId, Arg.Any<CancellationToken>())
            .Returns(cart);

        var command = new CreateCheckoutSessionCommand(CartId: _cartId, CustomerId: _customerId);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.ItemsCount.Should().Be(2);
        result.Items.Should().HaveCount(2);
        result.SubTotal.Should().Be(175m); // (50*2) + (75*1)
    }

    [Fact]
    public async Task Handle_CreatesCheckoutSession_WithVariants()
    {
        // Arrange
        var cart = ShoppingTestFixtures.CreateTestCustomerCart(_tenantId, _customerId, cartId: _cartId);
        var productId = Guid.NewGuid();
        var variantId = Guid.NewGuid();
        cart.AddItem(productId, 100m, 1, variantId);
        
        _cartRepository.GetByIdAsync(_cartId, Arg.Any<CancellationToken>())
            .Returns(cart);

        var command = new CreateCheckoutSessionCommand(CartId: _cartId, CustomerId: _customerId);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.Items.Should().HaveCount(1);
        result.Items[0].VariantId.Should().Be(variantId);
    }

    [Fact]
    public async Task Handle_CallsRepository_Add()
    {
        // Arrange
        var cart = ShoppingTestFixtures.CreateTestCustomerCart(_tenantId, _customerId, cartId: _cartId);
        cart.AddItem(Guid.NewGuid(), 50m, 1);
        
        _cartRepository.GetByIdAsync(_cartId, Arg.Any<CancellationToken>())
            .Returns(cart);

        var command = new CreateCheckoutSessionCommand(CartId: _cartId, CustomerId: _customerId);

        // Act
        await _handler.Handle(command, CancellationToken.None);

        // Assert
        _checkoutSessionRepository.Received(1).Add(Arg.Any<Domain.Shopping.Entities.CheckoutSession>());
    }

    [Fact]
    public async Task Handle_SavesChanges()
    {
        // Arrange
        var cart = ShoppingTestFixtures.CreateTestCustomerCart(_tenantId, _customerId, cartId: _cartId);
        cart.AddItem(Guid.NewGuid(), 50m, 1);
        
        _cartRepository.GetByIdAsync(_cartId, Arg.Any<CancellationToken>())
            .Returns(cart);

        var command = new CreateCheckoutSessionCommand(CartId: _cartId, CustomerId: _customerId);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        _checkoutSessionRepository.Received(1).Add(Arg.Any<Domain.Shopping.Entities.CheckoutSession>());
    }

    #endregion

    #region Validation Tests

    [Fact]
    public async Task Handle_CartNotFound_ThrowsException()
    {
        // Arrange
        _cartRepository.GetByIdAsync(_cartId, Arg.Any<CancellationToken>())
            .Returns((Domain.Shopping.Entities.Cart?)null);

        var command = new CreateCheckoutSessionCommand(CartId: _cartId, CustomerId: _customerId);

        // Act & Assert
        var act = () => _handler.Handle(command, CancellationToken.None);
        await act.Should().ThrowAsync<InvalidOperationException>()
            .WithMessage("*not found*");
    }

    [Fact]
    public async Task Handle_EmptyCart_ThrowsException()
    {
        // Arrange
        var cart = ShoppingTestFixtures.CreateTestCustomerCart(_tenantId, _customerId, cartId: _cartId);
        // Don't add any items
        
        _cartRepository.GetByIdAsync(_cartId, Arg.Any<CancellationToken>())
            .Returns(cart);

        var command = new CreateCheckoutSessionCommand(CartId: _cartId, CustomerId: _customerId);

        // Act & Assert
        var act = () => _handler.Handle(command, CancellationToken.None);
        await act.Should().ThrowAsync<InvalidOperationException>()
            .WithMessage("*empty cart*");
    }

    [Fact]
    public async Task Handle_CartBelongsToDifferentCustomer_ThrowsException()
    {
        // Arrange
        var otherCustomerId = Guid.NewGuid();
        var cart = ShoppingTestFixtures.CreateTestCustomerCart(_tenantId, otherCustomerId, cartId: _cartId);
        cart.AddItem(Guid.NewGuid(), 50m, 1);
        
        _cartRepository.GetByIdAsync(_cartId, Arg.Any<CancellationToken>())
            .Returns(cart);

        var command = new CreateCheckoutSessionCommand(CartId: _cartId, CustomerId: _customerId);

        // Act & Assert
        var act = () => _handler.Handle(command, CancellationToken.None);
        await act.Should().ThrowAsync<UnauthorizedAccessException>()
            .WithMessage("*does not belong*");
    }

    [Fact]
    public async Task Handle_CartFromDifferentTenant_ThrowsException()
    {
        // Arrange
        var otherTenantId = Guid.NewGuid();
        var cart = ShoppingTestFixtures.CreateTestCustomerCart(otherTenantId, _customerId, cartId: _cartId);
        cart.AddItem(Guid.NewGuid(), 50m, 1);
        
        _cartRepository.GetByIdAsync(_cartId, Arg.Any<CancellationToken>())
            .Returns(cart);

        var command = new CreateCheckoutSessionCommand(CartId: _cartId, CustomerId: _customerId);

        // Act & Assert
        var act = () => _handler.Handle(command, CancellationToken.None);
        await act.Should().ThrowAsync<UnauthorizedAccessException>()
            .WithMessage("*Cannot access cart from another tenant*");
    }

    [Fact]
    public async Task Handle_NullTenantContext_ThrowsException()
    {
        // Arrange
        var tenantContext = Substitute.For<ITenantContext>();
        tenantContext.TenantId.Returns((Guid?)null);

        var handler = new CreateCheckoutSessionCommandHandler(
            _cartRepository,
            _checkoutSessionRepository,
            _dbContext,
            Substitute.For<ILogger<CreateCheckoutSessionCommandHandler>>(),
            tenantContext);

        var cart = ShoppingTestFixtures.CreateTestCustomerCart(_tenantId, _customerId, cartId: _cartId);
        cart.AddItem(Guid.NewGuid(), 50m, 1);
        
        _cartRepository.GetByIdAsync(_cartId, Arg.Any<CancellationToken>())
            .Returns(cart);

        var command = new CreateCheckoutSessionCommand(CartId: _cartId, CustomerId: _customerId);

        // Act & Assert
        var act = () => handler.Handle(command, CancellationToken.None);
        await act.Should().ThrowAsync<InvalidOperationException>()
            .WithMessage("*Tenant context is not resolved*");
    }

    #endregion

    #region Response Validation Tests

    [Fact]
    public async Task Handle_Response_ContainsCorrectCustomerId()
    {
        // Arrange
        var cart = ShoppingTestFixtures.CreateTestCustomerCart(_tenantId, _customerId, cartId: _cartId);
        cart.AddItem(Guid.NewGuid(), 50m, 1);
        
        _cartRepository.GetByIdAsync(_cartId, Arg.Any<CancellationToken>())
            .Returns(cart);

        var command = new CreateCheckoutSessionCommand(CartId: _cartId, CustomerId: _customerId);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.CustomerId.Should().Be(_customerId);
        result.Currency.Should().Be("USD");
    }

    [Fact]
    public async Task Handle_Response_HasCreatedOnUtc()
    {
        // Arrange
        var cart = ShoppingTestFixtures.CreateTestCustomerCart(_tenantId, _customerId, cartId: _cartId);
        cart.AddItem(Guid.NewGuid(), 50m, 1);
        
        _cartRepository.GetByIdAsync(_cartId, Arg.Any<CancellationToken>())
            .Returns(cart);

        var command = new CreateCheckoutSessionCommand(CartId: _cartId, CustomerId: _customerId);

        // Act
        var beforeCall = DateTime.UtcNow;
        var result = await _handler.Handle(command, CancellationToken.None);
        var afterCall = DateTime.UtcNow;

        // Assert
        result.CreatedOnUtc.Should().BeAfter(beforeCall.AddSeconds(-1));
        result.CreatedOnUtc.Should().BeBefore(afterCall.AddSeconds(1));
    }

    [Fact]
    public async Task Handle_Response_CalculatesSubTotal()
    {
        // Arrange
        var cart = ShoppingTestFixtures.CreateTestCustomerCart(_tenantId, _customerId, cartId: _cartId);
        cart.AddItem(Guid.NewGuid(), 25.50m, 4);
        
        _cartRepository.GetByIdAsync(_cartId, Arg.Any<CancellationToken>())
            .Returns(cart);

        var command = new CreateCheckoutSessionCommand(CartId: _cartId, CustomerId: _customerId);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.SubTotal.Should().Be(102.00m); // 25.50 * 4
    }

    #endregion

    #region Large Cart Tests

    [Fact]
    public async Task Handle_WithManyItems_CreatesCheckoutSuccessfully()
    {
        // Arrange
        var cart = ShoppingTestFixtures.CreateTestCustomerCart(_tenantId, _customerId, cartId: _cartId);
        for (int i = 0; i < 10; i++)
        {
            cart.AddItem(Guid.NewGuid(), 10m + i, 1);
        }
        
        _cartRepository.GetByIdAsync(_cartId, Arg.Any<CancellationToken>())
            .Returns(cart);

        var command = new CreateCheckoutSessionCommand(CartId: _cartId, CustomerId: _customerId);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.ItemsCount.Should().Be(10);
        result.Items.Should().HaveCount(10);
    }

    [Fact]
    public async Task Handle_WithHighValueCart_MaintainsPrecision()
    {
        // Arrange
        var cart = ShoppingTestFixtures.CreateTestCustomerCart(_tenantId, _customerId, cartId: _cartId);
        cart.AddItem(Guid.NewGuid(), 999.99m, 3);
        cart.AddItem(Guid.NewGuid(), 1234.56m, 2);
        
        _cartRepository.GetByIdAsync(_cartId, Arg.Any<CancellationToken>())
            .Returns(cart);

        var command = new CreateCheckoutSessionCommand(CartId: _cartId, CustomerId: _customerId);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.SubTotal.Should().Be((999.99m * 3) + (1234.56m * 2)); // (999.99*3) + (1234.56*2) = 2999.97 + 2469.12 = 5469.09
    }

    #endregion

    #region Guest Cart Tests

    [Fact]
    public async Task Handle_WithGuestCart_ConvertedToCustomer_CreatesCheckoutSuccessfully()
    {
        // Arrange - Create a guest cart, then convert it to customer
        const string sessionId = "guest-session-123";
        var cart = ShoppingTestFixtures.CreateTestGuestCart(_tenantId, sessionId, cartId: _cartId);
        cart.AddItem(Guid.NewGuid(), 50m, 1);
        
        // Convert guest cart to customer cart
        cart.ConvertToCustomerCart(_customerId);
        
        _cartRepository.GetByIdAsync(_cartId, Arg.Any<CancellationToken>())
            .Returns(cart);

        var command = new CreateCheckoutSessionCommand(CartId: _cartId, CustomerId: _customerId);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.CustomerId.Should().Be(_customerId);
        result.ItemsCount.Should().Be(1);
    }

    #endregion

    #region Edge Cases and Boundary Tests

    [Fact]
    public async Task Handle_WithMinimumPrice_CreatesCheckout()
    {
        // Arrange
        var cart = ShoppingTestFixtures.CreateTestCustomerCart(_tenantId, _customerId, cartId: _cartId);
        cart.AddItem(Guid.NewGuid(), 0.01m, 1);
        
        _cartRepository.GetByIdAsync(_cartId, Arg.Any<CancellationToken>())
            .Returns(cart);

        var command = new CreateCheckoutSessionCommand(CartId: _cartId, CustomerId: _customerId);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.SubTotal.Should().Be(0.01m);
    }

    [Fact]
    public async Task Handle_WithHighQuantity_CreatesCheckout()
    {
        // Arrange
        var cart = ShoppingTestFixtures.CreateTestCustomerCart(_tenantId, _customerId, cartId: _cartId);
        cart.AddItem(Guid.NewGuid(), 10m, 999);
        
        _cartRepository.GetByIdAsync(_cartId, Arg.Any<CancellationToken>())
            .Returns(cart);

        var command = new CreateCheckoutSessionCommand(CartId: _cartId, CustomerId: _customerId);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.SubTotal.Should().Be(9990m);
    }

    [Fact]
    public async Task Handle_Response_Status_IsAlwaysDraft()
    {
        // Arrange
        var cart = ShoppingTestFixtures.CreateTestCustomerCart(_tenantId, _customerId, cartId: _cartId);
        cart.AddItem(Guid.NewGuid(), 50m, 1);
        
        _cartRepository.GetByIdAsync(_cartId, Arg.Any<CancellationToken>())
            .Returns(cart);

        var command = new CreateCheckoutSessionCommand(CartId: _cartId, CustomerId: _customerId);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.Status.Should().Be("Draft");
    }

    [Fact]
    public async Task Handle_WithMultipleTenants_IsolatesData()
    {
        // Arrange
        var tenant2Id = Guid.NewGuid();
        var cart = ShoppingTestFixtures.CreateTestCustomerCart(_tenantId, _customerId, cartId: _cartId);
        cart.AddItem(Guid.NewGuid(), 50m, 1);
        
        _cartRepository.GetByIdAsync(_cartId, Arg.Any<CancellationToken>())
            .Returns(cart);

        var command = new CreateCheckoutSessionCommand(CartId: _cartId, CustomerId: _customerId);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result.CustomerId.Should().Be(_customerId);
    }

    [Fact]
    public async Task Handle_WithDecimalPrices_MaintainsPrecision()
    {
        // Arrange
        var cart = ShoppingTestFixtures.CreateTestCustomerCart(_tenantId, _customerId, cartId: _cartId);
        cart.AddItem(Guid.NewGuid(), 19.99m, 3);
        cart.AddItem(Guid.NewGuid(), 9.99m, 5);
        
        _cartRepository.GetByIdAsync(_cartId, Arg.Any<CancellationToken>())
            .Returns(cart);

        var command = new CreateCheckoutSessionCommand(CartId: _cartId, CustomerId: _customerId);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.SubTotal.Should().Be((19.99m * 3) + (9.99m * 5));
    }

    [Fact]
    public async Task Handle_Repository_ReceivesCheckoutWithAllItems()
    {
        // Arrange
        var cart = ShoppingTestFixtures.CreateTestCustomerCart(_tenantId, _customerId, cartId: _cartId);
        var product1 = Guid.NewGuid();
        var product2 = Guid.NewGuid();
        cart.AddItem(product1, 50m, 2);
        cart.AddItem(product2, 75m, 1);
        
        _cartRepository.GetByIdAsync(_cartId, Arg.Any<CancellationToken>())
            .Returns(cart);

        var command = new CreateCheckoutSessionCommand(CartId: _cartId, CustomerId: _customerId);

        // Act
        await _handler.Handle(command, CancellationToken.None);

        // Assert
        _checkoutSessionRepository.Received(1).Add(Arg.Is<Domain.Shopping.Entities.CheckoutSession>(
            s => s.Items.Count == 2));
    }

    [Fact]
    public async Task Handle_WithCurrencyUSD_MatchesCartCurrency()
    {
        // Arrange
        var cart = ShoppingTestFixtures.CreateTestCustomerCart(_tenantId, _customerId, "USD", cartId: _cartId);
        cart.AddItem(Guid.NewGuid(), 50m, 1);
        
        _cartRepository.GetByIdAsync(_cartId, Arg.Any<CancellationToken>())
            .Returns(cart);

        var command = new CreateCheckoutSessionCommand(CartId: _cartId, CustomerId: _customerId);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.Currency.Should().Be("USD");
    }

    [Fact]
    public async Task Handle_AllItemsIncludedInCheckout()
    {
        // Arrange
        var cart = ShoppingTestFixtures.CreateTestCustomerCart(_tenantId, _customerId, cartId: _cartId);
        var ids = new[] { Guid.NewGuid(), Guid.NewGuid(), Guid.NewGuid(), Guid.NewGuid() };
        foreach (var id in ids)
        {
            cart.AddItem(id, 25m, 1);
        }
        
        _cartRepository.GetByIdAsync(_cartId, Arg.Any<CancellationToken>())
            .Returns(cart);

        var command = new CreateCheckoutSessionCommand(CartId: _cartId, CustomerId: _customerId);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.Items.Should().HaveCount(4);
        result.ItemsCount.Should().Be(4);
    }

    [Fact]
    public async Task Handle_GeneratesUniqueCheckoutSessionId()
    {
        // Arrange
        var cart1 = ShoppingTestFixtures.CreateTestCustomerCart(_tenantId, _customerId, cartId: _cartId);
        cart1.AddItem(Guid.NewGuid(), 50m, 1);
        
        var cart2Id = Guid.NewGuid();
        var cart2 = ShoppingTestFixtures.CreateTestCustomerCart(_tenantId, _customerId, cartId: cart2Id);
        cart2.AddItem(Guid.NewGuid(), 50m, 1);
        
        _cartRepository.GetByIdAsync(_cartId, Arg.Any<CancellationToken>())
            .Returns(cart1);
        _cartRepository.GetByIdAsync(cart2Id, Arg.Any<CancellationToken>())
            .Returns(cart2);

        var command1 = new CreateCheckoutSessionCommand(CartId: _cartId, CustomerId: _customerId);
        var command2 = new CreateCheckoutSessionCommand(CartId: cart2Id, CustomerId: _customerId);

        // Act
        var result1 = await _handler.Handle(command1, CancellationToken.None);
        var result2 = await _handler.Handle(command2, CancellationToken.None);

        // Assert
        result1.CheckoutSessionId.Should().NotBe(result2.CheckoutSessionId);
    }

    #endregion

    #region Stress and Performance Tests

    [Fact]
    public async Task Handle_WithManyItems_AllProcessedCorrectly()
    {
        // Arrange
        var cart = ShoppingTestFixtures.CreateTestCustomerCart(_tenantId, _customerId, cartId: _cartId);
        for (int i = 0; i < 50; i++)
        {
            cart.AddItem(Guid.NewGuid(), (decimal)(1 + i * 0.5), 1);
        }
        
        _cartRepository.GetByIdAsync(_cartId, Arg.Any<CancellationToken>())
            .Returns(cart);

        var command = new CreateCheckoutSessionCommand(CartId: _cartId, CustomerId: _customerId);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.ItemsCount.Should().Be(50);
        result.Items.Should().HaveCount(50);
    }

    [Fact]
    public async Task Handle_WithVariantCombinations_AllIncluded()
    {
        // Arrange
        var cart = ShoppingTestFixtures.CreateTestCustomerCart(_tenantId, _customerId, cartId: _cartId);
        var productId = Guid.NewGuid();
        var variantId1 = Guid.NewGuid();
        var variantId2 = Guid.NewGuid();
        cart.AddItem(productId, 50m, 2, variantId1);
        cart.AddItem(productId, 60m, 1, variantId2);
        
        _cartRepository.GetByIdAsync(_cartId, Arg.Any<CancellationToken>())
            .Returns(cart);

        var command = new CreateCheckoutSessionCommand(CartId: _cartId, CustomerId: _customerId);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.Items.Should().HaveCount(2);
        result.Items[0].VariantId.Should().Be(variantId1);
        result.Items[1].VariantId.Should().Be(variantId2);
    }

    [Fact]
    public async Task Handle_ConcurrencyScenario_CreatesDistinctCheckouts()
    {
        // Arrange
        var cart = ShoppingTestFixtures.CreateTestCustomerCart(_tenantId, _customerId, cartId: _cartId);
        cart.AddItem(Guid.NewGuid(), 50m, 1);
        
        _cartRepository.GetByIdAsync(_cartId, Arg.Any<CancellationToken>())
            .Returns(cart);

        var command1 = new CreateCheckoutSessionCommand(CartId: _cartId, CustomerId: _customerId);
        var command2 = new CreateCheckoutSessionCommand(CartId: _cartId, CustomerId: _customerId);

        // Act
        var result1 = await _handler.Handle(command1, CancellationToken.None);
        var result2 = await _handler.Handle(command2, CancellationToken.None);

        // Assert
        result1.CheckoutSessionId.Should().NotBe(result2.CheckoutSessionId);
        result1.SubTotal.Should().Be(result2.SubTotal);
    }

    #endregion
}
