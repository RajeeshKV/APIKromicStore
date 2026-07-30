using FluentAssertions;
using KromicStore.Application.Common.Abstractions;
using KromicStore.Application.Features.Shopping.Commands.SelectShippingMethod;
using KromicStore.Application.Features.Shopping.Abstractions;
using KromicStore.Application.Tests.Features.Shopping.Common;
using KromicStore.Infrastructure.Persistence;
using Microsoft.Extensions.Logging;
using NSubstitute;
using Xunit;

namespace KromicStore.Application.Tests.Features.Shopping.Commands.Checkout;

/// <summary>
/// Handler tests for SelectShippingMethodCommand.
/// Verifies selecting shipping methods for checkout sessions.
/// </summary>
public sealed class SelectShippingMethodCommandHandlerTests
{
    private readonly ICheckoutSessionRepository _checkoutSessionRepository;
    private readonly KromicStoreDbContext _dbContext;
    private readonly ITenantContext _tenantContext;
    private readonly SelectShippingMethodCommandHandler _handler;
    private readonly Guid _tenantId;
    private readonly Guid _customerId;
    private readonly Guid _checkoutSessionId;

    public SelectShippingMethodCommandHandlerTests()
    {
        _tenantId = Guid.NewGuid();
        _customerId = Guid.NewGuid();
        _checkoutSessionId = Guid.NewGuid();
        _dbContext = ShoppingTestFixtures.CreateDbContext(_tenantId);
        _tenantContext = ShoppingTestFixtures.CreateTenantContext(_tenantId);

        _checkoutSessionRepository = Substitute.For<ICheckoutSessionRepository>();

        _handler = new SelectShippingMethodCommandHandler(
            _checkoutSessionRepository,
            _dbContext,
            Substitute.For<ILogger<SelectShippingMethodCommandHandler>>(),
            _tenantContext);
    }

    #region Select Shipping Method Tests

    [Fact]
    public async Task Handle_SelectsShippingMethod_WithValidData()
    {
        // Arrange
        var session = ShoppingTestFixtures.CreateTestCheckoutSession(_tenantId, _customerId, _checkoutSessionId);
        session.AddItem(Guid.NewGuid(), 50m, 1);
        session.SetShippingAddress(Guid.NewGuid());
        
        _checkoutSessionRepository.GetByIdAsync(_checkoutSessionId, Arg.Any<CancellationToken>())
            .Returns(session);

        var command = new SelectShippingMethodCommand(
            CheckoutSessionId: _checkoutSessionId,
            ShippingMethodId: "standard",
            ShippingCost: 10.00m);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result.CheckoutSessionId.Should().Be(_checkoutSessionId);
        result.ShippingMethodId.Should().Be("standard");
        result.ShippingCost.Should().Be(10.00m);
    }

    [Fact]
    public async Task Handle_SelectsMultipleShippingMethods()
    {
        // Arrange
        var session = ShoppingTestFixtures.CreateTestCheckoutSession(_tenantId, _customerId, _checkoutSessionId);
        session.AddItem(Guid.NewGuid(), 100m, 1);
        session.SetShippingAddress(Guid.NewGuid());
        
        _checkoutSessionRepository.GetByIdAsync(_checkoutSessionId, Arg.Any<CancellationToken>())
            .Returns(session);

        var shippingMethods = new[]
        {
            ("standard", 10.00m),
            ("express", 25.00m),
            ("overnight", 50.00m),
            ("free", 0.00m),
        };

        foreach (var (method, cost) in shippingMethods)
        {
            var command = new SelectShippingMethodCommand(
                CheckoutSessionId: _checkoutSessionId,
                ShippingMethodId: method,
                ShippingCost: cost);

            // Act
            var result = await _handler.Handle(command, CancellationToken.None);

            // Assert
            result.ShippingMethodId.Should().Be(method);
            result.ShippingCost.Should().Be(cost);
        }
    }

    [Fact]
    public async Task Handle_CalculatesCorrectTotal_WithShippingCost()
    {
        // Arrange
        var session = ShoppingTestFixtures.CreateTestCheckoutSession(_tenantId, _customerId, _checkoutSessionId);
        session.AddItem(Guid.NewGuid(), 100m, 1);
        session.SetShippingAddress(Guid.NewGuid());
        
        _checkoutSessionRepository.GetByIdAsync(_checkoutSessionId, Arg.Any<CancellationToken>())
            .Returns(session);

        var command = new SelectShippingMethodCommand(
            CheckoutSessionId: _checkoutSessionId,
            ShippingMethodId: "standard",
            ShippingCost: 15.00m);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.Total.Should().Be(115.00m); // 100 + 15
    }

    [Fact]
    public async Task Handle_SavesChanges()
    {
        // Arrange
        var session = ShoppingTestFixtures.CreateTestCheckoutSession(_tenantId, _customerId, _checkoutSessionId);
        session.AddItem(Guid.NewGuid(), 50m, 1);
        session.SetShippingAddress(Guid.NewGuid());
        
        _checkoutSessionRepository.GetByIdAsync(_checkoutSessionId, Arg.Any<CancellationToken>())
            .Returns(session);

        var command = new SelectShippingMethodCommand(
            CheckoutSessionId: _checkoutSessionId,
            ShippingMethodId: "express",
            ShippingCost: 25.00m);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.ShippingCost.Should().Be(25.00m);
    }

    #endregion

    #region Validation Tests

    [Fact]
    public async Task Handle_CheckoutSessionNotFound_ThrowsException()
    {
        // Arrange
        _checkoutSessionRepository.GetByIdAsync(_checkoutSessionId, Arg.Any<CancellationToken>())
            .Returns((Domain.Shopping.Entities.CheckoutSession?)null);

        var command = new SelectShippingMethodCommand(
            CheckoutSessionId: _checkoutSessionId,
            ShippingMethodId: "standard",
            ShippingCost: 10.00m);

        // Act & Assert
        var act = () => _handler.Handle(command, CancellationToken.None);
        await act.Should().ThrowAsync<InvalidOperationException>()
            .WithMessage("*not found*");
    }

    [Fact]
    public async Task Handle_CheckoutSessionFromDifferentTenant_ThrowsException()
    {
        // Arrange
        var otherTenantId = Guid.NewGuid();
        var session = ShoppingTestFixtures.CreateTestCheckoutSession(otherTenantId, _customerId, _checkoutSessionId);
        session.AddItem(Guid.NewGuid(), 50m, 1);
        session.SetShippingAddress(Guid.NewGuid());
        
        _checkoutSessionRepository.GetByIdAsync(_checkoutSessionId, Arg.Any<CancellationToken>())
            .Returns(session);

        var command = new SelectShippingMethodCommand(
            CheckoutSessionId: _checkoutSessionId,
            ShippingMethodId: "standard",
            ShippingCost: 10.00m);

        // Act & Assert
        var act = () => _handler.Handle(command, CancellationToken.None);
        await act.Should().ThrowAsync<UnauthorizedAccessException>()
            .WithMessage("*Cannot access checkout session from another tenant*");
    }

    [Fact]
    public async Task Handle_ShippingAddressNotSet_ThrowsException()
    {
        // Arrange
        var session = ShoppingTestFixtures.CreateTestCheckoutSession(_tenantId, _customerId, _checkoutSessionId);
        session.AddItem(Guid.NewGuid(), 50m, 1);
        // Don't set shipping address
        
        _checkoutSessionRepository.GetByIdAsync(_checkoutSessionId, Arg.Any<CancellationToken>())
            .Returns(session);

        var command = new SelectShippingMethodCommand(
            CheckoutSessionId: _checkoutSessionId,
            ShippingMethodId: "standard",
            ShippingCost: 10.00m);

        // Act & Assert
        var act = () => _handler.Handle(command, CancellationToken.None);
        await act.Should().ThrowAsync<InvalidOperationException>()
            .WithMessage("*Shipping address must be set*");
    }

    [Fact]
    public async Task Handle_NullTenantContext_ThrowsException()
    {
        // Arrange
        var tenantContext = Substitute.For<ITenantContext>();
        tenantContext.TenantId.Returns((Guid?)null);

        var handler = new SelectShippingMethodCommandHandler(
            _checkoutSessionRepository,
            _dbContext,
            Substitute.For<ILogger<SelectShippingMethodCommandHandler>>(),
            tenantContext);

        var session = ShoppingTestFixtures.CreateTestCheckoutSession(_tenantId, _customerId, _checkoutSessionId);
        session.AddItem(Guid.NewGuid(), 50m, 1);
        session.SetShippingAddress(Guid.NewGuid());
        
        _checkoutSessionRepository.GetByIdAsync(_checkoutSessionId, Arg.Any<CancellationToken>())
            .Returns(session);

        var command = new SelectShippingMethodCommand(
            CheckoutSessionId: _checkoutSessionId,
            ShippingMethodId: "standard",
            ShippingCost: 10.00m);

        // Act & Assert
        var act = () => handler.Handle(command, CancellationToken.None);
        await act.Should().ThrowAsync<InvalidOperationException>()
            .WithMessage("*Tenant context is not resolved*");
    }

    #endregion

    #region Shipping Cost Calculation Tests

    [Fact]
    public async Task Handle_CalculatesCorrectTotal_WithComplexCart()
    {
        // Arrange
        var session = ShoppingTestFixtures.CreateTestCheckoutSession(_tenantId, _customerId, _checkoutSessionId);
        session.AddItem(Guid.NewGuid(), 50m, 2);
        session.AddItem(Guid.NewGuid(), 30m, 3);
        session.SetShippingAddress(Guid.NewGuid());
        
        _checkoutSessionRepository.GetByIdAsync(_checkoutSessionId, Arg.Any<CancellationToken>())
            .Returns(session);

        var command = new SelectShippingMethodCommand(
            CheckoutSessionId: _checkoutSessionId,
            ShippingMethodId: "express",
            ShippingCost: 20.00m);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        // SubTotal: (50*2) + (30*3) = 100 + 90 = 190
        // Total: 190 + 20 = 210
        result.Total.Should().Be(210.00m);
    }

    [Fact]
    public async Task Handle_WithFreeShipping_CalculatesCorrectTotal()
    {
        // Arrange
        var session = ShoppingTestFixtures.CreateTestCheckoutSession(_tenantId, _customerId, _checkoutSessionId);
        session.AddItem(Guid.NewGuid(), 50m, 1);
        session.SetShippingAddress(Guid.NewGuid());
        
        _checkoutSessionRepository.GetByIdAsync(_checkoutSessionId, Arg.Any<CancellationToken>())
            .Returns(session);

        var command = new SelectShippingMethodCommand(
            CheckoutSessionId: _checkoutSessionId,
            ShippingMethodId: "free",
            ShippingCost: 0.00m);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.Total.Should().Be(50.00m);
        result.ShippingCost.Should().Be(0.00m);
    }

    [Fact]
    public async Task Handle_WithHighShippingCost_MaintainsPrecision()
    {
        // Arrange
        var session = ShoppingTestFixtures.CreateTestCheckoutSession(_tenantId, _customerId, _checkoutSessionId);
        session.AddItem(Guid.NewGuid(), 999.99m, 1);
        session.SetShippingAddress(Guid.NewGuid());
        
        _checkoutSessionRepository.GetByIdAsync(_checkoutSessionId, Arg.Any<CancellationToken>())
            .Returns(session);

        var command = new SelectShippingMethodCommand(
            CheckoutSessionId: _checkoutSessionId,
            ShippingMethodId: "overnight",
            ShippingCost: 99.99m);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.Total.Should().Be(1099.98m);
    }

    #endregion

    #region Response Validation Tests

    [Fact]
    public async Task Handle_Response_HasCorrectCheckoutSessionId()
    {
        // Arrange
        var session = ShoppingTestFixtures.CreateTestCheckoutSession(_tenantId, _customerId, _checkoutSessionId);
        session.AddItem(Guid.NewGuid(), 50m, 1);
        session.SetShippingAddress(Guid.NewGuid());
        
        _checkoutSessionRepository.GetByIdAsync(_checkoutSessionId, Arg.Any<CancellationToken>())
            .Returns(session);

        var command = new SelectShippingMethodCommand(
            CheckoutSessionId: _checkoutSessionId,
            ShippingMethodId: "standard",
            ShippingCost: 10.00m);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.CheckoutSessionId.Should().Be(_checkoutSessionId);
    }

    [Fact]
    public async Task Handle_Response_ContainsShippingMethodDetails()
    {
        // Arrange
        var session = ShoppingTestFixtures.CreateTestCheckoutSession(_tenantId, _customerId, _checkoutSessionId);
        session.AddItem(Guid.NewGuid(), 50m, 1);
        session.SetShippingAddress(Guid.NewGuid());
        
        _checkoutSessionRepository.GetByIdAsync(_checkoutSessionId, Arg.Any<CancellationToken>())
            .Returns(session);

        const string methodId = "express";
        const decimal cost = 25.50m;

        var command = new SelectShippingMethodCommand(
            CheckoutSessionId: _checkoutSessionId,
            ShippingMethodId: methodId,
            ShippingCost: cost);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.ShippingMethodId.Should().Be(methodId);
        result.ShippingCost.Should().Be(cost);
    }

    #endregion

    #region State Management Tests

    [Fact]
    public async Task Handle_UpdatesMultipleTimes_LastOneWins()
    {
        // Arrange
        var session = ShoppingTestFixtures.CreateTestCheckoutSession(_tenantId, _customerId, _checkoutSessionId);
        session.AddItem(Guid.NewGuid(), 50m, 1);
        session.SetShippingAddress(Guid.NewGuid());
        
        _checkoutSessionRepository.GetByIdAsync(_checkoutSessionId, Arg.Any<CancellationToken>())
            .Returns(session);

        var command1 = new SelectShippingMethodCommand(
            CheckoutSessionId: _checkoutSessionId,
            ShippingMethodId: "standard",
            ShippingCost: 10.00m);

        var command2 = new SelectShippingMethodCommand(
            CheckoutSessionId: _checkoutSessionId,
            ShippingMethodId: "express",
            ShippingCost: 25.00m);

        // Act
        var result1 = await _handler.Handle(command1, CancellationToken.None);
        var result2 = await _handler.Handle(command2, CancellationToken.None);

        // Assert
        result2.ShippingMethodId.Should().Be("express");
        result2.ShippingCost.Should().Be(25.00m);
    }

    #endregion

    #region Additional Edge Cases

    [Fact]
    public async Task Handle_WithZeroShippingCost_CalculatesCorrectly()
    {
        // Arrange
        var session = ShoppingTestFixtures.CreateTestCheckoutSession(_tenantId, _customerId, _checkoutSessionId);
        session.AddItem(Guid.NewGuid(), 100m, 1);
        session.SetShippingAddress(Guid.NewGuid());
        
        _checkoutSessionRepository.GetByIdAsync(_checkoutSessionId, Arg.Any<CancellationToken>())
            .Returns(session);

        var command = new SelectShippingMethodCommand(
            CheckoutSessionId: _checkoutSessionId,
            ShippingMethodId: "free-pickup",
            ShippingCost: 0m);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.ShippingCost.Should().Be(0m);
        result.Total.Should().Be(100m);
    }

    [Fact]
    public async Task Handle_WithHighShippingCostAndMultipleItems_MaintainsPrecision()
    {
        // Arrange
        var session = ShoppingTestFixtures.CreateTestCheckoutSession(_tenantId, _customerId, _checkoutSessionId);
        session.AddItem(Guid.NewGuid(), 99.99m, 2);
        session.SetShippingAddress(Guid.NewGuid());
        
        _checkoutSessionRepository.GetByIdAsync(_checkoutSessionId, Arg.Any<CancellationToken>())
            .Returns(session);

        var command = new SelectShippingMethodCommand(
            CheckoutSessionId: _checkoutSessionId,
            ShippingMethodId: "international",
            ShippingCost: 149.99m);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.Total.Should().Be(199.98m + 149.99m); // (99.99*2) + 149.99
    }

    [Fact]
    public async Task Handle_WithDecimalShippingCost_MaintainsPrecision()
    {
        // Arrange
        var session = ShoppingTestFixtures.CreateTestCheckoutSession(_tenantId, _customerId, _checkoutSessionId);
        session.AddItem(Guid.NewGuid(), 50m, 1);
        session.SetShippingAddress(Guid.NewGuid());
        
        _checkoutSessionRepository.GetByIdAsync(_checkoutSessionId, Arg.Any<CancellationToken>())
            .Returns(session);

        var command = new SelectShippingMethodCommand(
            CheckoutSessionId: _checkoutSessionId,
            ShippingMethodId: "standard",
            ShippingCost: 7.99m);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.Total.Should().Be(57.99m);
    }

    [Fact]
    public async Task Handle_ShippingCheckoutSessionNotFound_ThrowsException()
    {
        // Arrange
        _checkoutSessionRepository.GetByIdAsync(_checkoutSessionId, Arg.Any<CancellationToken>())
            .Returns((Domain.Shopping.Entities.CheckoutSession?)null);

        var command = new SelectShippingMethodCommand(
            CheckoutSessionId: _checkoutSessionId,
            ShippingMethodId: "any",
            ShippingCost: 10m);

        // Act & Assert
        var act = () => _handler.Handle(command, CancellationToken.None);
        await act.Should().ThrowAsync<InvalidOperationException>();
    }

    [Fact]
    public async Task Handle_ShippingMethodIdAlwaysSet()
    {
        // Arrange
        var session = ShoppingTestFixtures.CreateTestCheckoutSession(_tenantId, _customerId, _checkoutSessionId);
        session.AddItem(Guid.NewGuid(), 50m, 1);
        session.SetShippingAddress(Guid.NewGuid());
        
        _checkoutSessionRepository.GetByIdAsync(_checkoutSessionId, Arg.Any<CancellationToken>())
            .Returns(session);

        var command = new SelectShippingMethodCommand(
            CheckoutSessionId: _checkoutSessionId,
            ShippingMethodId: "custom-method",
            ShippingCost: 12.50m);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.ShippingMethodId.Should().Be("custom-method");
    }

    [Fact]
    public async Task Handle_WithMultipleShippingSelections_LastOneWins()
    {
        // Arrange
        var session = ShoppingTestFixtures.CreateTestCheckoutSession(_tenantId, _customerId, _checkoutSessionId);
        session.AddItem(Guid.NewGuid(), 50m, 1);
        session.SetShippingAddress(Guid.NewGuid());
        
        _checkoutSessionRepository.GetByIdAsync(_checkoutSessionId, Arg.Any<CancellationToken>())
            .Returns(session);

        var command1 = new SelectShippingMethodCommand(
            CheckoutSessionId: _checkoutSessionId,
            ShippingMethodId: "method1",
            ShippingCost: 5m);

        var command2 = new SelectShippingMethodCommand(
            CheckoutSessionId: _checkoutSessionId,
            ShippingMethodId: "method2",
            ShippingCost: 10m);

        // Act
        await _handler.Handle(command1, CancellationToken.None);
        var result2 = await _handler.Handle(command2, CancellationToken.None);

        // Assert
        result2.ShippingMethodId.Should().Be("method2");
        result2.ShippingCost.Should().Be(10m);
    }

    #endregion
}
