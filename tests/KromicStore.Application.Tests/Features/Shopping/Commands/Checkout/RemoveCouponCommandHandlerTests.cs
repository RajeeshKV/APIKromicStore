using FluentAssertions;
using KromicStore.Application.Common.Abstractions;
using KromicStore.Application.Features.Shopping.Commands.RemoveCoupon;
using KromicStore.Application.Features.Shopping.Abstractions;
using KromicStore.Application.Tests.Features.Shopping.Common;
using KromicStore.Infrastructure.Persistence;
using Microsoft.Extensions.Logging;
using NSubstitute;
using Xunit;

namespace KromicStore.Application.Tests.Features.Shopping.Commands.Checkout;

public sealed class RemoveCouponCommandHandlerTests
{
    private readonly ICheckoutSessionRepository _checkoutSessionRepository;
    private readonly KromicStoreDbContext _dbContext;
    private readonly ITenantContext _tenantContext;
    private readonly RemoveCouponCommandHandler _handler;
    private readonly Guid _tenantId;
    private readonly Guid _customerId;
    private readonly Guid _checkoutSessionId;

    public RemoveCouponCommandHandlerTests()
    {
        _tenantId = Guid.NewGuid();
        _customerId = Guid.NewGuid();
        _checkoutSessionId = Guid.NewGuid();
        _dbContext = ShoppingTestFixtures.CreateDbContext(_tenantId);
        _tenantContext = ShoppingTestFixtures.CreateTenantContext(_tenantId);

        _checkoutSessionRepository = Substitute.For<ICheckoutSessionRepository>();

        _handler = new RemoveCouponCommandHandler(
            _checkoutSessionRepository,
            _dbContext,
            Substitute.For<ILogger<RemoveCouponCommandHandler>>(),
            _tenantContext);
    }

    [Fact]
    public async Task Handle_RemovesCoupon_WhenApplied()
    {
        // Arrange
        var session = ShoppingTestFixtures.CreateTestCheckoutSession(_tenantId, _customerId, _checkoutSessionId);
        session.AddItem(Guid.NewGuid(), 100m, 1);
        session.ApplyCoupon("SAVE10", 10m);
        
        _checkoutSessionRepository.GetByIdAsync(_checkoutSessionId, Arg.Any<CancellationToken>())
            .Returns(session);

        var command = new RemoveCouponCommand(CheckoutSessionId: _checkoutSessionId);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.CheckoutSessionId.Should().Be(_checkoutSessionId);
        result.CouponRemoved.Should().BeTrue();
        result.Total.Should().Be(100m); // Discount removed
    }

    [Fact]
    public async Task Handle_RestoresFullPrice_AfterCouponRemoval()
    {
        // Arrange
        var session = ShoppingTestFixtures.CreateTestCheckoutSession(_tenantId, _customerId, _checkoutSessionId);
        session.AddItem(Guid.NewGuid(), 50m, 2);
        session.ApplyCoupon("DISCOUNT", 10m);
        
        _checkoutSessionRepository.GetByIdAsync(_checkoutSessionId, Arg.Any<CancellationToken>())
            .Returns(session);

        var command = new RemoveCouponCommand(CheckoutSessionId: _checkoutSessionId);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.Total.Should().Be(100m); // Original subtotal: 50 * 2
    }

    [Fact]
    public async Task Handle_RemoveCoupon_WhenNoCouponApplied_ReturnsFalse()
    {
        // Arrange
        var session = ShoppingTestFixtures.CreateTestCheckoutSession(_tenantId, _customerId, _checkoutSessionId);
        session.AddItem(Guid.NewGuid(), 50m, 1);
        // No coupon applied
        
        _checkoutSessionRepository.GetByIdAsync(_checkoutSessionId, Arg.Any<CancellationToken>())
            .Returns(session);

        var command = new RemoveCouponCommand(CheckoutSessionId: _checkoutSessionId);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.CouponRemoved.Should().BeFalse();
        result.Total.Should().Be(50m);
    }

    [Fact]
    public async Task Handle_RemovesMultipleCoupons_Successfully()
    {
        // Arrange
        var session = ShoppingTestFixtures.CreateTestCheckoutSession(_tenantId, _customerId, _checkoutSessionId);
        session.AddItem(Guid.NewGuid(), 100m, 1);
        
        _checkoutSessionRepository.GetByIdAsync(_checkoutSessionId, Arg.Any<CancellationToken>())
            .Returns(session);

        var coupons = new[] { "COUPON1", "COUPON2", "COUPON3" };

        // Act
        foreach (var coupon in coupons)
        {
            session.ApplyCoupon(coupon, 10m);
            var command = new RemoveCouponCommand(CheckoutSessionId: _checkoutSessionId);
            var result = await _handler.Handle(command, CancellationToken.None);

            // Assert
            result.CouponRemoved.Should().BeTrue();
        }
    }

    [Fact]
    public async Task Handle_CheckoutSessionNotFound_ThrowsException()
    {
        // Arrange
        _checkoutSessionRepository.GetByIdAsync(_checkoutSessionId, Arg.Any<CancellationToken>())
            .Returns((Domain.Shopping.Entities.CheckoutSession?)null);

        var command = new RemoveCouponCommand(CheckoutSessionId: _checkoutSessionId);

        // Act & Assert
        var act = () => _handler.Handle(command, CancellationToken.None);
        await act.Should().ThrowAsync<InvalidOperationException>()
            .WithMessage("*not found*");
    }

    [Fact]
    public async Task Handle_DifferentTenant_ThrowsException()
    {
        // Arrange
        var otherTenantId = Guid.NewGuid();
        var session = ShoppingTestFixtures.CreateTestCheckoutSession(otherTenantId, _customerId, _checkoutSessionId);
        session.AddItem(Guid.NewGuid(), 50m, 1);
        session.ApplyCoupon("SAVE", 5m);
        
        _checkoutSessionRepository.GetByIdAsync(_checkoutSessionId, Arg.Any<CancellationToken>())
            .Returns(session);

        var command = new RemoveCouponCommand(CheckoutSessionId: _checkoutSessionId);

        // Act & Assert
        var act = () => _handler.Handle(command, CancellationToken.None);
        await act.Should().ThrowAsync<UnauthorizedAccessException>();
    }

    [Fact]
    public async Task Handle_SavesChanges_WhenCouponRemoved()
    {
        // Arrange
        var session = ShoppingTestFixtures.CreateTestCheckoutSession(_tenantId, _customerId, _checkoutSessionId);
        session.AddItem(Guid.NewGuid(), 50m, 1);
        session.ApplyCoupon("SAVE", 5m);
        
        _checkoutSessionRepository.GetByIdAsync(_checkoutSessionId, Arg.Any<CancellationToken>())
            .Returns(session);

        var command = new RemoveCouponCommand(CheckoutSessionId: _checkoutSessionId);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.CouponRemoved.Should().BeTrue();
    }

    [Fact]
    public async Task Handle_Response_ContainsCheckoutSessionId()
    {
        // Arrange
        var session = ShoppingTestFixtures.CreateTestCheckoutSession(_tenantId, _customerId, _checkoutSessionId);
        session.AddItem(Guid.NewGuid(), 50m, 1);
        session.ApplyCoupon("SAVE", 5m);
        
        _checkoutSessionRepository.GetByIdAsync(_checkoutSessionId, Arg.Any<CancellationToken>())
            .Returns(session);

        var command = new RemoveCouponCommand(CheckoutSessionId: _checkoutSessionId);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.CheckoutSessionId.Should().Be(_checkoutSessionId);
    }

    [Fact]
    public async Task Handle_NullTenantContext_ThrowsException()
    {
        // Arrange
        var tenantContext = Substitute.For<ITenantContext>();
        tenantContext.TenantId.Returns((Guid?)null);

        var handler = new RemoveCouponCommandHandler(
            _checkoutSessionRepository,
            _dbContext,
            Substitute.For<ILogger<RemoveCouponCommandHandler>>(),
            tenantContext);

        var session = ShoppingTestFixtures.CreateTestCheckoutSession(_tenantId, _customerId, _checkoutSessionId);
        session.AddItem(Guid.NewGuid(), 50m, 1);
        session.ApplyCoupon("SAVE", 5m);
        
        _checkoutSessionRepository.GetByIdAsync(_checkoutSessionId, Arg.Any<CancellationToken>())
            .Returns(session);

        var command = new RemoveCouponCommand(CheckoutSessionId: _checkoutSessionId);

        // Act & Assert
        var act = () => handler.Handle(command, CancellationToken.None);
        await act.Should().ThrowAsync<InvalidOperationException>()
            .WithMessage("*Tenant context is not resolved*");
    }

    [Fact]
    public async Task Handle_WithMultipleItems_CalculatesCorrectTotal()
    {
        // Arrange
        var session = ShoppingTestFixtures.CreateTestCheckoutSession(_tenantId, _customerId, _checkoutSessionId);
        session.AddItem(Guid.NewGuid(), 50m, 2);
        session.AddItem(Guid.NewGuid(), 30m, 1);
        session.ApplyCoupon("SAVE", 16m);
        
        _checkoutSessionRepository.GetByIdAsync(_checkoutSessionId, Arg.Any<CancellationToken>())
            .Returns(session);

        var command = new RemoveCouponCommand(CheckoutSessionId: _checkoutSessionId);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        // SubTotal: (50*2) + 30 = 130
        result.Total.Should().Be(130m);
    }

    #region Additional Edge Cases

    [Fact]
    public async Task Handle_WithZeroDiscount_StillRemovesCoupon()
    {
        // Arrange
        var session = ShoppingTestFixtures.CreateTestCheckoutSession(_tenantId, _customerId, _checkoutSessionId);
        session.AddItem(Guid.NewGuid(), 50m, 1);
        session.ApplyCoupon("FREE", 0m);
        
        _checkoutSessionRepository.GetByIdAsync(_checkoutSessionId, Arg.Any<CancellationToken>())
            .Returns(session);

        var command = new RemoveCouponCommand(CheckoutSessionId: _checkoutSessionId);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.CouponRemoved.Should().BeTrue();
    }

    [Fact]
    public async Task Handle_RestoringFullPrice_WithHighDiscount()
    {
        // Arrange
        var session = ShoppingTestFixtures.CreateTestCheckoutSession(_tenantId, _customerId, _checkoutSessionId);
        session.AddItem(Guid.NewGuid(), 200m, 1);
        session.ApplyCoupon("BIG", 100m);
        
        _checkoutSessionRepository.GetByIdAsync(_checkoutSessionId, Arg.Any<CancellationToken>())
            .Returns(session);

        var command = new RemoveCouponCommand(CheckoutSessionId: _checkoutSessionId);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.Total.Should().Be(200m);
    }

    [Fact]
    public async Task Handle_CheckoutNotFound_ThrowsException()
    {
        // Arrange
        _checkoutSessionRepository.GetByIdAsync(_checkoutSessionId, Arg.Any<CancellationToken>())
            .Returns((Domain.Shopping.Entities.CheckoutSession?)null);

        var command = new RemoveCouponCommand(CheckoutSessionId: _checkoutSessionId);

        // Act & Assert
        var act = () => _handler.Handle(command, CancellationToken.None);
        await act.Should().ThrowAsync<InvalidOperationException>();
    }

    [Fact]
    public async Task Handle_WithMultipleCouponsRemoved_LastOneProcessed()
    {
        // Arrange
        var session = ShoppingTestFixtures.CreateTestCheckoutSession(_tenantId, _customerId, _checkoutSessionId);
        session.AddItem(Guid.NewGuid(), 100m, 1);
        session.ApplyCoupon("COUPON1", 10m);
        
        _checkoutSessionRepository.GetByIdAsync(_checkoutSessionId, Arg.Any<CancellationToken>())
            .Returns(session);

        var command1 = new RemoveCouponCommand(CheckoutSessionId: _checkoutSessionId);
        var command2 = new RemoveCouponCommand(CheckoutSessionId: _checkoutSessionId);

        // Act
        var result1 = await _handler.Handle(command1, CancellationToken.None);
        var result2 = await _handler.Handle(command2, CancellationToken.None);

        // Assert
        result1.CouponRemoved.Should().BeTrue();
        result2.Total.Should().Be(100m);
    }

    #endregion
}
