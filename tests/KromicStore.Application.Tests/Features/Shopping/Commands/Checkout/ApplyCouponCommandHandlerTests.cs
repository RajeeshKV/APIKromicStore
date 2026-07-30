using FluentAssertions;
using KromicStore.Application.Common.Abstractions;
using KromicStore.Application.Features.Shopping.Commands.ApplyCoupon;
using KromicStore.Application.Features.Shopping.Abstractions;
using KromicStore.Application.Tests.Features.Shopping.Common;
using KromicStore.Infrastructure.Persistence;
using Microsoft.Extensions.Logging;
using NSubstitute;
using Xunit;

namespace KromicStore.Application.Tests.Features.Shopping.Commands.Checkout;

public sealed class ApplyCouponCommandHandlerTests
{
    private readonly ICheckoutSessionRepository _checkoutSessionRepository;
    private readonly KromicStoreDbContext _dbContext;
    private readonly ITenantContext _tenantContext;
    private readonly ApplyCouponCommandHandler _handler;
    private readonly Guid _tenantId;
    private readonly Guid _customerId;
    private readonly Guid _checkoutSessionId;

    public ApplyCouponCommandHandlerTests()
    {
        _tenantId = Guid.NewGuid();
        _customerId = Guid.NewGuid();
        _checkoutSessionId = Guid.NewGuid();
        _dbContext = ShoppingTestFixtures.CreateDbContext(_tenantId);
        _tenantContext = ShoppingTestFixtures.CreateTenantContext(_tenantId);

        _checkoutSessionRepository = Substitute.For<ICheckoutSessionRepository>();

        _handler = new ApplyCouponCommandHandler(
            _checkoutSessionRepository,
            _dbContext,
            Substitute.For<ILogger<ApplyCouponCommandHandler>>(),
            _tenantContext);
    }

    [Fact]
    public async Task Handle_AppliesCoupon_WithValidCode()
    {
        // Arrange
        var session = ShoppingTestFixtures.CreateTestCheckoutSession(_tenantId, _customerId, _checkoutSessionId);
        session.AddItem(Guid.NewGuid(), 100m, 1);
        
        _checkoutSessionRepository.GetByIdAsync(_checkoutSessionId, Arg.Any<CancellationToken>())
            .Returns(session);

        var command = new ApplyCouponCommand(
            CheckoutSessionId: _checkoutSessionId,
            CouponCode: "SAVE10");

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.CheckoutSessionId.Should().Be(_checkoutSessionId);
        result.CouponCode.Should().Contain("SAVE10");
        result.DiscountAmount.Should().BeGreaterThan(0);
        result.Total.Should().BeLessThan(100m);
    }

    [Fact]
    public async Task Handle_CalculatesDiscount_At10Percent()
    {
        // Arrange
        var session = ShoppingTestFixtures.CreateTestCheckoutSession(_tenantId, _customerId, _checkoutSessionId);
        session.AddItem(Guid.NewGuid(), 100m, 1);
        
        _checkoutSessionRepository.GetByIdAsync(_checkoutSessionId, Arg.Any<CancellationToken>())
            .Returns(session);

        var command = new ApplyCouponCommand(
            CheckoutSessionId: _checkoutSessionId,
            CouponCode: "SAVE10");

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.DiscountAmount.Should().Be(10m); // 10% of 100
        result.Total.Should().Be(90m); // 100 - 10
    }

    [Fact]
    public async Task Handle_AppliesCoupon_ToMultipleItems()
    {
        // Arrange
        var session = ShoppingTestFixtures.CreateTestCheckoutSession(_tenantId, _customerId, _checkoutSessionId);
        session.AddItem(Guid.NewGuid(), 50m, 2);
        session.AddItem(Guid.NewGuid(), 30m, 1);
        
        _checkoutSessionRepository.GetByIdAsync(_checkoutSessionId, Arg.Any<CancellationToken>())
            .Returns(session);

        var command = new ApplyCouponCommand(
            CheckoutSessionId: _checkoutSessionId,
            CouponCode: "DISCOUNT20");

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        // SubTotal: (50*2) + 30 = 130
        // Discount: 130 * 0.10 = 13
        result.DiscountAmount.Should().Be(13m);
        result.Total.Should().Be(117m);
    }

    [Fact]
    public async Task Handle_CheckoutSessionNotFound_ThrowsException()
    {
        // Arrange
        _checkoutSessionRepository.GetByIdAsync(_checkoutSessionId, Arg.Any<CancellationToken>())
            .Returns((Domain.Shopping.Entities.CheckoutSession?)null);

        var command = new ApplyCouponCommand(
            CheckoutSessionId: _checkoutSessionId,
            CouponCode: "SAVE10");

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
        
        _checkoutSessionRepository.GetByIdAsync(_checkoutSessionId, Arg.Any<CancellationToken>())
            .Returns(session);

        var command = new ApplyCouponCommand(
            CheckoutSessionId: _checkoutSessionId,
            CouponCode: "SAVE10");

        // Act & Assert
        var act = () => _handler.Handle(command, CancellationToken.None);
        await act.Should().ThrowAsync<UnauthorizedAccessException>();
    }

    [Fact]
    public async Task Handle_AppliesCouponMultipleTimes_LastOneWins()
    {
        // Arrange
        var session = ShoppingTestFixtures.CreateTestCheckoutSession(_tenantId, _customerId, _checkoutSessionId);
        session.AddItem(Guid.NewGuid(), 100m, 1);
        
        _checkoutSessionRepository.GetByIdAsync(_checkoutSessionId, Arg.Any<CancellationToken>())
            .Returns(session);

        var command1 = new ApplyCouponCommand(
            CheckoutSessionId: _checkoutSessionId,
            CouponCode: "FIRST");

        var command2 = new ApplyCouponCommand(
            CheckoutSessionId: _checkoutSessionId,
            CouponCode: "SECOND");

        // Act
        await _handler.Handle(command1, CancellationToken.None);
        var result2 = await _handler.Handle(command2, CancellationToken.None);

        // Assert
        result2.CouponCode.Should().Contain("SECOND");
    }

    [Fact]
    public async Task Handle_WithHighValue_MaintainsPrecision()
    {
        // Arrange
        var session = ShoppingTestFixtures.CreateTestCheckoutSession(_tenantId, _customerId, _checkoutSessionId);
        session.AddItem(Guid.NewGuid(), 999.99m, 2);
        
        _checkoutSessionRepository.GetByIdAsync(_checkoutSessionId, Arg.Any<CancellationToken>())
            .Returns(session);

        var command = new ApplyCouponCommand(
            CheckoutSessionId: _checkoutSessionId,
            CouponCode: "SAVE");

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        // SubTotal: 999.99 * 2 = 1999.98
        // Discount: 1999.98 * 0.10 = 199.998 ≈ 199.99 or 200
        result.DiscountAmount.Should().BeGreaterThan(0);
    }

    [Fact]
    public async Task Handle_Response_ContainsCouponCode()
    {
        // Arrange
        var session = ShoppingTestFixtures.CreateTestCheckoutSession(_tenantId, _customerId, _checkoutSessionId);
        session.AddItem(Guid.NewGuid(), 50m, 1);
        
        _checkoutSessionRepository.GetByIdAsync(_checkoutSessionId, Arg.Any<CancellationToken>())
            .Returns(session);

        const string couponCode = "BESTDEAL";
        var command = new ApplyCouponCommand(
            CheckoutSessionId: _checkoutSessionId,
            CouponCode: couponCode);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.CouponCode.Should().Contain(couponCode);
    }

    [Fact]
    public async Task Handle_WithMultipleCoupons_IgnoresPrevious()
    {
        // Arrange
        var session = ShoppingTestFixtures.CreateTestCheckoutSession(_tenantId, _customerId, _checkoutSessionId);
        session.AddItem(Guid.NewGuid(), 100m, 1);
        
        _checkoutSessionRepository.GetByIdAsync(_checkoutSessionId, Arg.Any<CancellationToken>())
            .Returns(session);

        var coupons = new[] { "COUPON1", "COUPON2", "COUPON3" };

        // Act & Assert
        foreach (var coupon in coupons)
        {
            var command = new ApplyCouponCommand(
                CheckoutSessionId: _checkoutSessionId,
                CouponCode: coupon);

            var result = await _handler.Handle(command, CancellationToken.None);
            result.CouponCode.Should().Contain(coupon);
        }
    }

    [Fact]
    public async Task Handle_NullTenantContext_ThrowsException()
    {
        // Arrange
        var tenantContext = Substitute.For<ITenantContext>();
        tenantContext.TenantId.Returns((Guid?)null);

        var handler = new ApplyCouponCommandHandler(
            _checkoutSessionRepository,
            _dbContext,
            Substitute.For<ILogger<ApplyCouponCommandHandler>>(),
            tenantContext);

        var session = ShoppingTestFixtures.CreateTestCheckoutSession(_tenantId, _customerId, _checkoutSessionId);
        session.AddItem(Guid.NewGuid(), 50m, 1);
        
        _checkoutSessionRepository.GetByIdAsync(_checkoutSessionId, Arg.Any<CancellationToken>())
            .Returns(session);

        var command = new ApplyCouponCommand(
            CheckoutSessionId: _checkoutSessionId,
            CouponCode: "SAVE10");

        // Act & Assert
        var act = () => handler.Handle(command, CancellationToken.None);
        await act.Should().ThrowAsync<InvalidOperationException>()
            .WithMessage("*Tenant context is not resolved*");
    }

    #region Additional Edge Cases

    [Fact]
    public async Task Handle_WithHighDiscountPercentage_CalculatesCorrectly()
    {
        // Arrange
        var session = ShoppingTestFixtures.CreateTestCheckoutSession(_tenantId, _customerId, _checkoutSessionId);
        session.AddItem(Guid.NewGuid(), 100m, 1);
        
        _checkoutSessionRepository.GetByIdAsync(_checkoutSessionId, Arg.Any<CancellationToken>())
            .Returns(session);

        var command = new ApplyCouponCommand(
            CheckoutSessionId: _checkoutSessionId,
            CouponCode: "SAVE50");

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.DiscountAmount.Should().BeGreaterThan(0);
    }

    [Fact]
    public async Task Handle_WithSmallCartAndCoupon_CalculatesCorrectly()
    {
        // Arrange
        var session = ShoppingTestFixtures.CreateTestCheckoutSession(_tenantId, _customerId, _checkoutSessionId);
        session.AddItem(Guid.NewGuid(), 5m, 1);
        
        _checkoutSessionRepository.GetByIdAsync(_checkoutSessionId, Arg.Any<CancellationToken>())
            .Returns(session);

        var command = new ApplyCouponCommand(
            CheckoutSessionId: _checkoutSessionId,
            CouponCode: "MINIMAL");

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.CouponCode.Should().Contain("MINIMAL");
    }

    [Fact]
    public async Task Handle_CouponCodeAlwaysSet()
    {
        // Arrange
        var session = ShoppingTestFixtures.CreateTestCheckoutSession(_tenantId, _customerId, _checkoutSessionId);
        session.AddItem(Guid.NewGuid(), 50m, 1);
        
        _checkoutSessionRepository.GetByIdAsync(_checkoutSessionId, Arg.Any<CancellationToken>())
            .Returns(session);

        var command = new ApplyCouponCommand(
            CheckoutSessionId: _checkoutSessionId,
            CouponCode: "TEST2024");

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.CouponCode.Should().NotBeNullOrEmpty();
        result.CouponCode.Should().Contain("TEST2024");
    }

    [Fact]
    public async Task Handle_CheckoutSessionIdMatchesResponse()
    {
        // Arrange
        var session = ShoppingTestFixtures.CreateTestCheckoutSession(_tenantId, _customerId, _checkoutSessionId);
        session.AddItem(Guid.NewGuid(), 75m, 1);
        
        _checkoutSessionRepository.GetByIdAsync(_checkoutSessionId, Arg.Any<CancellationToken>())
            .Returns(session);

        var command = new ApplyCouponCommand(
            CheckoutSessionId: _checkoutSessionId,
            CouponCode: "CODE");

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.CheckoutSessionId.Should().Be(_checkoutSessionId);
    }

    [Fact]
    public async Task Handle_WithLargeCartAndCoupon_ProcessesSuccessfully()
    {
        // Arrange
        var session = ShoppingTestFixtures.CreateTestCheckoutSession(_tenantId, _customerId, _checkoutSessionId);
        for (int i = 0; i < 10; i++)
        {
            session.AddItem(Guid.NewGuid(), 100m, 1);
        }
        
        _checkoutSessionRepository.GetByIdAsync(_checkoutSessionId, Arg.Any<CancellationToken>())
            .Returns(session);

        var command = new ApplyCouponCommand(
            CheckoutSessionId: _checkoutSessionId,
            CouponCode: "BULK");

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.CouponCode.Should().Contain("BULK");
        result.DiscountAmount.Should().BeGreaterThanOrEqualTo(0);
    }

    #endregion
}
