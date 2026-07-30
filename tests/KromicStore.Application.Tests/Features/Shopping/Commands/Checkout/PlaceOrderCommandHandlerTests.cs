using FluentAssertions;
using KromicStore.Application.Common.Abstractions;
using KromicStore.Application.Features.Shopping.Commands.PlaceOrder;
using KromicStore.Application.Features.Shopping.Abstractions;
using KromicStore.Application.Tests.Features.Shopping.Common;
using KromicStore.Domain.Shopping.Entities;
using KromicStore.Infrastructure.Persistence;
using Microsoft.Extensions.Logging;
using NSubstitute;
using Xunit;

namespace KromicStore.Application.Tests.Features.Shopping.Commands.Checkout;

public sealed class PlaceOrderCommandHandlerTests
{
    private readonly ICheckoutSessionRepository _checkoutSessionRepository;
    private readonly KromicStoreDbContext _dbContext;
    private readonly ITenantContext _tenantContext;
    private readonly PlaceOrderCommandHandler _handler;
    private readonly Guid _tenantId;
    private readonly Guid _customerId;
    private readonly Guid _checkoutSessionId;

    public PlaceOrderCommandHandlerTests()
    {
        _tenantId = Guid.NewGuid();
        _customerId = Guid.NewGuid();
        _checkoutSessionId = Guid.NewGuid();
        _dbContext = ShoppingTestFixtures.CreateDbContext(_tenantId);
        _tenantContext = ShoppingTestFixtures.CreateTenantContext(_tenantId);

        _checkoutSessionRepository = Substitute.For<ICheckoutSessionRepository>();

        _handler = new PlaceOrderCommandHandler(
            _checkoutSessionRepository,
            _dbContext,
            Substitute.For<ILogger<PlaceOrderCommandHandler>>(),
            _tenantContext);
    }

    [Fact]
    public async Task Handle_PlacesOrder_WithValidCheckout()
    {
        // Arrange
        var session = ShoppingTestFixtures.CreateTestCheckoutSession(_tenantId, _customerId, _checkoutSessionId);
        session.AddItem(Guid.NewGuid(), 100m, 1);
        session.SetShippingAddress(Guid.NewGuid());
        session.SetShippingMethod("standard", 10m);
        session.SetBillingAddress(Guid.NewGuid());
        session.SetPaymentMethod("card");
        session.AwaitPayment();
        
        _checkoutSessionRepository.GetByIdAsync(_checkoutSessionId, Arg.Any<CancellationToken>())
            .Returns(session);

        var command = new PlaceOrderCommand(
            CheckoutSessionId: _checkoutSessionId,
            PaymentTransactionId: "TXN-123456");

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result.CustomerId.Should().Be(_customerId);
        result.Items.Should().HaveCount(1);
        result.SubTotal.Should().Be(100m);
    }

    [Fact]
    public async Task Handle_GeneratesOrderNumber()
    {
        // Arrange
        var session = ShoppingTestFixtures.CreateTestCheckoutSession(_tenantId, _customerId, _checkoutSessionId);
        session.AddItem(Guid.NewGuid(), 50m, 1);
        session.SetShippingAddress(Guid.NewGuid());
        session.SetShippingMethod("express", 15m);
        session.SetBillingAddress(Guid.NewGuid());
        session.SetPaymentMethod("card");
        session.AwaitPayment();
        
        _checkoutSessionRepository.GetByIdAsync(_checkoutSessionId, Arg.Any<CancellationToken>())
            .Returns(session);

        var command = new PlaceOrderCommand(
            CheckoutSessionId: _checkoutSessionId,
            PaymentTransactionId: "TXN-789012");

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.OrderNumber.Should().StartWith("ORD-");
        result.OrderNumber.Should().Contain(DateTime.UtcNow.ToString("yyyyMMdd"));
    }

    [Fact]
    public async Task Handle_PlacesOrder_WithMultipleItems()
    {
        // Arrange
        var session = ShoppingTestFixtures.CreateTestCheckoutSession(_tenantId, _customerId, _checkoutSessionId);
        session.AddItem(Guid.NewGuid(), 50m, 2);
        session.AddItem(Guid.NewGuid(), 30m, 1);
        session.SetShippingAddress(Guid.NewGuid());
        session.SetShippingMethod("standard", 10m);
        session.SetBillingAddress(Guid.NewGuid());
        session.SetPaymentMethod("card");
        session.AwaitPayment();
        
        _checkoutSessionRepository.GetByIdAsync(_checkoutSessionId, Arg.Any<CancellationToken>())
            .Returns(session);

        var command = new PlaceOrderCommand(
            CheckoutSessionId: _checkoutSessionId,
            PaymentTransactionId: "TXN-345678");

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.Items.Should().HaveCount(2);
        result.SubTotal.Should().Be(130m); // (50*2) + 30
    }

    [Fact]
    public async Task Handle_CalculatesOrderTotal_WithShippingAndDiscount()
    {
        // Arrange
        var session = ShoppingTestFixtures.CreateTestCheckoutSession(_tenantId, _customerId, _checkoutSessionId);
        session.AddItem(Guid.NewGuid(), 100m, 1);
        session.SetShippingAddress(Guid.NewGuid());
        session.SetShippingMethod("express", 25m);
        session.ApplyCoupon("SAVE", 10m);
        session.SetBillingAddress(Guid.NewGuid());
        session.SetPaymentMethod("card");
        session.AwaitPayment();
        
        _checkoutSessionRepository.GetByIdAsync(_checkoutSessionId, Arg.Any<CancellationToken>())
            .Returns(session);

        var command = new PlaceOrderCommand(
            CheckoutSessionId: _checkoutSessionId,
            PaymentTransactionId: "TXN-901234");

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        // SubTotal: 100, Shipping: 25, Discount: 10, Total: 100 + 25 - 10 = 115
        result.SubTotal.Should().Be(100m);
        result.ShippingCost.Should().Be(25m);
        result.DiscountAmount.Should().Be(10m);
        result.Total.Should().Be(115m);
    }

    [Fact]
    public async Task Handle_CheckoutSessionNotFound_ThrowsException()
    {
        // Arrange
        _checkoutSessionRepository.GetByIdAsync(_checkoutSessionId, Arg.Any<CancellationToken>())
            .Returns((CheckoutSession?)null);

        var command = new PlaceOrderCommand(
            CheckoutSessionId: _checkoutSessionId,
            PaymentTransactionId: "TXN-123456");

        // Act & Assert
        var act = () => _handler.Handle(command, CancellationToken.None);
        await act.Should().ThrowAsync<InvalidOperationException>()
            .WithMessage("*not found*");
    }

    [Fact]
    public async Task Handle_CheckoutNotInAwaitingPaymentState_ThrowsException()
    {
        // Arrange
        var session = ShoppingTestFixtures.CreateTestCheckoutSession(_tenantId, _customerId, _checkoutSessionId);
        session.AddItem(Guid.NewGuid(), 50m, 1);
        // Session is in Draft state, not AwaitingPayment
        
        _checkoutSessionRepository.GetByIdAsync(_checkoutSessionId, Arg.Any<CancellationToken>())
            .Returns(session);

        var command = new PlaceOrderCommand(
            CheckoutSessionId: _checkoutSessionId,
            PaymentTransactionId: "TXN-123456");

        // Act & Assert
        var act = () => _handler.Handle(command, CancellationToken.None);
        await act.Should().ThrowAsync<InvalidOperationException>()
            .WithMessage("*AwaitingPayment*");
    }

    [Fact]
    public async Task Handle_PaymentMethodNotSet_ThrowsException()
    {
        // Arrange
        var session = ShoppingTestFixtures.CreateTestCheckoutSession(_tenantId, _customerId, _checkoutSessionId);
        session.AddItem(Guid.NewGuid(), 50m, 1);
        session.SetShippingAddress(Guid.NewGuid());
        session.SetShippingMethod("standard", 10m);
        session.SetBillingAddress(Guid.NewGuid());
        // Don't set payment method
        session.AwaitPayment();
        
        _checkoutSessionRepository.GetByIdAsync(_checkoutSessionId, Arg.Any<CancellationToken>())
            .Returns(session);

        var command = new PlaceOrderCommand(
            CheckoutSessionId: _checkoutSessionId,
            PaymentTransactionId: "TXN-123456");

        // Act & Assert
        var act = () => _handler.Handle(command, CancellationToken.None);
        await act.Should().ThrowAsync<InvalidOperationException>()
            .WithMessage("*Payment*");
    }

    [Fact]
    public async Task Handle_DifferentTenant_ThrowsException()
    {
        // Arrange
        var otherTenantId = Guid.NewGuid();
        var session = ShoppingTestFixtures.CreateTestCheckoutSession(otherTenantId, _customerId, _checkoutSessionId);
        session.AddItem(Guid.NewGuid(), 50m, 1);
        session.SetShippingAddress(Guid.NewGuid());
        session.SetShippingMethod("standard", 10m);
        session.SetBillingAddress(Guid.NewGuid());
        session.SetPaymentMethod("card");
        session.AwaitPayment();
        
        _checkoutSessionRepository.GetByIdAsync(_checkoutSessionId, Arg.Any<CancellationToken>())
            .Returns(session);

        var command = new PlaceOrderCommand(
            CheckoutSessionId: _checkoutSessionId,
            PaymentTransactionId: "TXN-123456");

        // Act & Assert
        var act = () => _handler.Handle(command, CancellationToken.None);
        await act.Should().ThrowAsync<UnauthorizedAccessException>();
    }

    [Fact]
    public async Task Handle_Response_ContainsOrderDetails()
    {
        // Arrange
        var session = ShoppingTestFixtures.CreateTestCheckoutSession(_tenantId, _customerId, _checkoutSessionId);
        var productId = Guid.NewGuid();
        session.AddItem(productId, 75m, 2);
        session.SetShippingAddress(Guid.NewGuid());
        session.SetShippingMethod("standard", 10m);
        session.SetBillingAddress(Guid.NewGuid());
        session.SetPaymentMethod("card");
        session.AwaitPayment();
        
        _checkoutSessionRepository.GetByIdAsync(_checkoutSessionId, Arg.Any<CancellationToken>())
            .Returns(session);

        var command = new PlaceOrderCommand(
            CheckoutSessionId: _checkoutSessionId,
            PaymentTransactionId: "TXN-555555");

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.CustomerId.Should().Be(_customerId);
        result.Items[0].ProductId.Should().Be(productId);
        result.Items[0].Quantity.Should().Be(2);
        result.Items[0].UnitPrice.Should().Be(75m);
        result.Status.Should().Be("Completed");
    }

    [Fact]
    public async Task Handle_NullTenantContext_ThrowsException()
    {
        // Arrange
        var tenantContext = Substitute.For<ITenantContext>();
        tenantContext.TenantId.Returns((Guid?)null);

        var handler = new PlaceOrderCommandHandler(
            _checkoutSessionRepository,
            _dbContext,
            Substitute.For<ILogger<PlaceOrderCommandHandler>>(),
            tenantContext);

        var session = ShoppingTestFixtures.CreateTestCheckoutSession(_tenantId, _customerId, _checkoutSessionId);
        session.AddItem(Guid.NewGuid(), 50m, 1);
        session.SetShippingAddress(Guid.NewGuid());
        session.SetShippingMethod("standard", 10m);
        session.SetBillingAddress(Guid.NewGuid());
        session.SetPaymentMethod("card");
        session.AwaitPayment();
        
        _checkoutSessionRepository.GetByIdAsync(_checkoutSessionId, Arg.Any<CancellationToken>())
            .Returns(session);

        var command = new PlaceOrderCommand(
            CheckoutSessionId: _checkoutSessionId,
            PaymentTransactionId: "TXN-123456");

        // Act & Assert
        var act = () => handler.Handle(command, CancellationToken.None);
        await act.Should().ThrowAsync<InvalidOperationException>()
            .WithMessage("*Tenant context is not resolved*");
    }

    [Fact]
    public async Task Handle_WithComplexOrder_MaintainsPrecision()
    {
        // Arrange
        var session = ShoppingTestFixtures.CreateTestCheckoutSession(_tenantId, _customerId, _checkoutSessionId);
        session.AddItem(Guid.NewGuid(), 99.99m, 3);
        session.AddItem(Guid.NewGuid(), 49.99m, 2);
        session.SetShippingAddress(Guid.NewGuid());
        session.SetShippingMethod("express", 24.99m);
        session.ApplyCoupon("BIGDISCOUNT", 50m);
        session.SetBillingAddress(Guid.NewGuid());
        session.SetPaymentMethod("card");
        session.AwaitPayment();
        
        _checkoutSessionRepository.GetByIdAsync(_checkoutSessionId, Arg.Any<CancellationToken>())
            .Returns(session);

        var command = new PlaceOrderCommand(
            CheckoutSessionId: _checkoutSessionId,
            PaymentTransactionId: "TXN-999999");

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        // SubTotal: (99.99*3) + (49.99*2) = 299.97 + 99.98 = 399.95
        result.SubTotal.Should().Be(399.95m);
        result.DiscountAmount.Should().Be(50m);
    }

    [Fact]
    public async Task Handle_Response_HasCorrectCreatedOnUtc()
    {
        // Arrange
        var beforeTime = DateTime.UtcNow;
        var session = ShoppingTestFixtures.CreateTestCheckoutSession(_tenantId, _customerId, _checkoutSessionId);
        session.AddItem(Guid.NewGuid(), 50m, 1);
        session.SetShippingAddress(Guid.NewGuid());
        session.SetShippingMethod("standard", 10m);
        session.SetBillingAddress(Guid.NewGuid());
        session.SetPaymentMethod("card");
        session.AwaitPayment();
        
        _checkoutSessionRepository.GetByIdAsync(_checkoutSessionId, Arg.Any<CancellationToken>())
            .Returns(session);

        var command = new PlaceOrderCommand(
            CheckoutSessionId: _checkoutSessionId,
            PaymentTransactionId: "TXN-111111");

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);
        var afterTime = DateTime.UtcNow;

        // Assert
        result.CreatedOnUtc.Should().BeAfter(beforeTime.AddSeconds(-1));
        result.CreatedOnUtc.Should().BeBefore(afterTime.AddSeconds(1));
    }

    #region Additional Edge Cases

    [Fact]
    public async Task Handle_WithMinimalOrder_PlacesSuccessfully()
    {
        // Arrange
        var session = ShoppingTestFixtures.CreateTestCheckoutSession(_tenantId, _customerId, _checkoutSessionId);
        session.AddItem(Guid.NewGuid(), 0.99m, 1);
        session.SetShippingAddress(Guid.NewGuid());
        session.SetShippingMethod("pickup", 0m);
        session.SetBillingAddress(Guid.NewGuid());
        session.SetPaymentMethod("card");
        session.AwaitPayment();
        
        _checkoutSessionRepository.GetByIdAsync(_checkoutSessionId, Arg.Any<CancellationToken>())
            .Returns(session);

        var command = new PlaceOrderCommand(
            CheckoutSessionId: _checkoutSessionId,
            PaymentTransactionId: "TXN-MINIMAL");

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.Total.Should().BeGreaterThan(0);
    }

    [Fact]
    public async Task Handle_WithLargeQuantity_CalculatesCorrectly()
    {
        // Arrange
        var session = ShoppingTestFixtures.CreateTestCheckoutSession(_tenantId, _customerId, _checkoutSessionId);
        session.AddItem(Guid.NewGuid(), 10m, 100);
        session.SetShippingAddress(Guid.NewGuid());
        session.SetShippingMethod("bulk", 50m);
        session.SetBillingAddress(Guid.NewGuid());
        session.SetPaymentMethod("card");
        session.AwaitPayment();
        
        _checkoutSessionRepository.GetByIdAsync(_checkoutSessionId, Arg.Any<CancellationToken>())
            .Returns(session);

        var command = new PlaceOrderCommand(
            CheckoutSessionId: _checkoutSessionId,
            PaymentTransactionId: "TXN-BULK");

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.SubTotal.Should().Be(1000m); // 10 * 100
        result.Total.Should().Be(1050m); // 1000 + 50 shipping
    }

    [Fact]
    public async Task Handle_WithLargeOrder_ContainsCompleteOrderDetails()
    {
        // Arrange
        var session = ShoppingTestFixtures.CreateTestCheckoutSession(_tenantId, _customerId, _checkoutSessionId);
        session.AddItem(Guid.NewGuid(), 75m, 2);
        session.SetShippingAddress(Guid.NewGuid());
        session.SetShippingMethod("standard", 15m);
        session.SetBillingAddress(Guid.NewGuid());
        session.SetPaymentMethod("card");
        session.AwaitPayment();
        
        _checkoutSessionRepository.GetByIdAsync(_checkoutSessionId, Arg.Any<CancellationToken>())
            .Returns(session);

        var command = new PlaceOrderCommand(
            CheckoutSessionId: _checkoutSessionId,
            PaymentTransactionId: "TXN-DETAILS");

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.OrderNumber.Should().NotBeNullOrEmpty();
        result.Status.Should().NotBeNullOrEmpty();
    }

    [Fact]
    public async Task Handle_OrderHasCorrectCustomerId()
    {
        // Arrange
        var session = ShoppingTestFixtures.CreateTestCheckoutSession(_tenantId, _customerId, _checkoutSessionId);
        session.AddItem(Guid.NewGuid(), 50m, 1);
        session.SetShippingAddress(Guid.NewGuid());
        session.SetShippingMethod("standard", 10m);
        session.SetBillingAddress(Guid.NewGuid());
        session.SetPaymentMethod("card");
        session.AwaitPayment();
        
        _checkoutSessionRepository.GetByIdAsync(_checkoutSessionId, Arg.Any<CancellationToken>())
            .Returns(session);

        var command = new PlaceOrderCommand(
            CheckoutSessionId: _checkoutSessionId,
            PaymentTransactionId: "TXN-USER");

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.CustomerId.Should().Be(_customerId);
    }

    [Fact]
    public async Task Handle_OrderNumberIsUnique()
    {
        // Arrange
        var session1 = ShoppingTestFixtures.CreateTestCheckoutSession(_tenantId, _customerId, _checkoutSessionId);
        session1.AddItem(Guid.NewGuid(), 50m, 1);
        session1.SetShippingAddress(Guid.NewGuid());
        session1.SetShippingMethod("standard", 10m);
        session1.SetBillingAddress(Guid.NewGuid());
        session1.SetPaymentMethod("card");
        session1.AwaitPayment();

        var session2Id = Guid.NewGuid();
        var session2 = ShoppingTestFixtures.CreateTestCheckoutSession(_tenantId, _customerId, session2Id);
        session2.AddItem(Guid.NewGuid(), 50m, 1);
        session2.SetShippingAddress(Guid.NewGuid());
        session2.SetShippingMethod("standard", 10m);
        session2.SetBillingAddress(Guid.NewGuid());
        session2.SetPaymentMethod("card");
        session2.AwaitPayment();
        
        _checkoutSessionRepository.GetByIdAsync(_checkoutSessionId, Arg.Any<CancellationToken>())
            .Returns(session1);
        _checkoutSessionRepository.GetByIdAsync(session2Id, Arg.Any<CancellationToken>())
            .Returns(session2);

        var command1 = new PlaceOrderCommand(
            CheckoutSessionId: _checkoutSessionId,
            PaymentTransactionId: "TXN-1");
        var command2 = new PlaceOrderCommand(
            CheckoutSessionId: session2Id,
            PaymentTransactionId: "TXN-2");

        // Act
        var result1 = await _handler.Handle(command1, CancellationToken.None);
        var result2 = await _handler.Handle(command2, CancellationToken.None);

        // Assert
        result1.OrderNumber.Should().NotBe(result2.OrderNumber);
    }

    [Fact]
    public async Task Handle_OrderIncludesAllItems()
    {
        // Arrange
        var session = ShoppingTestFixtures.CreateTestCheckoutSession(_tenantId, _customerId, _checkoutSessionId);
        session.AddItem(Guid.NewGuid(), 25m, 2);
        session.AddItem(Guid.NewGuid(), 30m, 1);
        session.AddItem(Guid.NewGuid(), 15m, 3);
        session.SetShippingAddress(Guid.NewGuid());
        session.SetShippingMethod("standard", 10m);
        session.SetBillingAddress(Guid.NewGuid());
        session.SetPaymentMethod("card");
        session.AwaitPayment();
        
        _checkoutSessionRepository.GetByIdAsync(_checkoutSessionId, Arg.Any<CancellationToken>())
            .Returns(session);

        var command = new PlaceOrderCommand(
            CheckoutSessionId: _checkoutSessionId,
            PaymentTransactionId: "TXN-ITEMS");

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.Items.Should().HaveCount(3);
    }

    #endregion
}
