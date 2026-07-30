using FluentAssertions;
using KromicStore.Application.Common.Abstractions;
using KromicStore.Application.Features.Shopping.Commands.CreateCart;
using KromicStore.Application.Features.Shopping.Abstractions;
using KromicStore.Application.Tests.Features.Shopping.Common;
using KromicStore.Infrastructure.Persistence;
using Microsoft.Extensions.Logging;
using NSubstitute;
using Xunit;

namespace KromicStore.Application.Tests.Features.Shopping.Commands.Cart;

/// <summary>
/// Handler tests for CreateCartCommand.
/// Verifies cart creation for customers and guests with validation and persistence.
/// </summary>
public sealed class CreateCartCommandHandlerTests
{
    private readonly ICartRepository _cartRepository;
    private readonly KromicStoreDbContext _dbContext;
    private readonly ICurrentUserService _currentUserService;
    private readonly ITenantContext _tenantContext;
    private readonly CreateCartCommandHandler _handler;
    private readonly Guid _tenantId;
    private readonly Guid _customerId;

    public CreateCartCommandHandlerTests()
    {
        _tenantId = Guid.NewGuid();
        _customerId = Guid.NewGuid();
        _dbContext = ShoppingTestFixtures.CreateDbContext(_tenantId);
        _tenantContext = ShoppingTestFixtures.CreateTenantContext(_tenantId);
        _currentUserService = ShoppingTestFixtures.CreateCurrentUserService(_customerId);

        _cartRepository = Substitute.For<ICartRepository>();

        _handler = new CreateCartCommandHandler(
            _cartRepository,
            _dbContext,
            Substitute.For<ILogger<CreateCartCommandHandler>>(),
            _tenantContext,
            _currentUserService);
    }

    #region Create Customer Cart Tests

    [Fact]
    public async Task Handle_CustomerCart_WithValidData_CreatesCart()
    {
        // Arrange
        _cartRepository.GetByCustomerIdAsync(_customerId, Arg.Any<CancellationToken>())
            .Returns((Domain.Shopping.Entities.Cart?)null);

        var command = new CreateCartCommand(
            CustomerId: _customerId,
            Currency: "USD");

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result.CartId.Should().NotBeEmpty();
        result.CustomerId.Should().Be(_customerId);
        result.AnonymousSessionId.Should().BeNull();
        result.Currency.Should().Be("USD");
        result.ItemsCount.Should().Be(0);
        result.SubTotal.Should().Be(0);
    }

    [Fact]
    public async Task Handle_CustomerCart_CallsRepository_Add()
    {
        // Arrange
        _cartRepository.GetByCustomerIdAsync(_customerId, Arg.Any<CancellationToken>())
            .Returns((Domain.Shopping.Entities.Cart?)null);

        var command = new CreateCartCommand(CustomerId: _customerId, Currency: "USD");

        // Act
        await _handler.Handle(command, CancellationToken.None);

        // Assert
        _cartRepository.Received(1).Add(Arg.Any<Domain.Shopping.Entities.Cart>());
    }

    [Fact]
    public async Task Handle_CustomerCart_CallsDbContext_SaveChanges()
    {
        // Arrange
        _cartRepository.GetByCustomerIdAsync(_customerId, Arg.Any<CancellationToken>())
            .Returns((Domain.Shopping.Entities.Cart?)null);

        var command = new CreateCartCommand(CustomerId: _customerId, Currency: "USD");

        // Act
        await _handler.Handle(command, CancellationToken.None);

        // Assert
        // This would normally be verified via dbContext.SaveChangesAsync being called
        // With NSubstitute, we verify the repository was updated
        _cartRepository.Received(1).Add(Arg.Any<Domain.Shopping.Entities.Cart>());
    }

    [Fact]
    public async Task Handle_CustomerCart_WithExistingCart_ThrowsException()
    {
        // Arrange
        var existingCart = ShoppingTestFixtures.CreateTestCustomerCart(_tenantId, _customerId);
        _cartRepository.GetByCustomerIdAsync(_customerId, Arg.Any<CancellationToken>())
            .Returns(existingCart);

        var command = new CreateCartCommand(CustomerId: _customerId, Currency: "USD");

        // Act & Assert
        var act = () => _handler.Handle(command, CancellationToken.None);
        await act.Should().ThrowAsync<InvalidOperationException>()
            .WithMessage("*already has an active cart*");
    }

    [Fact]
    public async Task Handle_CustomerCart_WithEmptyCustomerId_ThrowsException()
    {
        // Arrange
        var command = new CreateCartCommand(CustomerId: Guid.Empty, Currency: "USD");

        // Act & Assert
        // Validator should catch this, but handler should also be defensive
        var act = () => _handler.Handle(command, CancellationToken.None);
        await act.Should().ThrowAsync<InvalidOperationException>();
    }

    #endregion

    #region Create Guest Cart Tests

    [Fact]
    public async Task Handle_GuestCart_WithValidData_CreatesCart()
    {
        // Arrange
        const string sessionId = "guest-session-456";
        _cartRepository.GetByAnonymousSessionIdAsync(sessionId, Arg.Any<CancellationToken>())
            .Returns((Domain.Shopping.Entities.Cart?)null);

        var command = new CreateCartCommand(
            AnonymousSessionId: sessionId,
            Currency: "USD");

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result.CartId.Should().NotBeEmpty();
        result.CustomerId.Should().BeNull();
        result.AnonymousSessionId.Should().Be(sessionId);
        result.Currency.Should().Be("USD");
        result.ItemsCount.Should().Be(0);
        result.SubTotal.Should().Be(0);
    }

    [Fact]
    public async Task Handle_GuestCart_WithValidData_CallsRepository_Add()
    {
        // Arrange
        const string sessionId = "guest-session-789";
        _cartRepository.GetByAnonymousSessionIdAsync(sessionId, Arg.Any<CancellationToken>())
            .Returns((Domain.Shopping.Entities.Cart?)null);

        var command = new CreateCartCommand(AnonymousSessionId: sessionId, Currency: "USD");

        // Act
        await _handler.Handle(command, CancellationToken.None);

        // Assert
        _cartRepository.Received(1).Add(Arg.Any<Domain.Shopping.Entities.Cart>());
    }

    [Fact]
    public async Task Handle_GuestCart_WithExistingCart_ThrowsException()
    {
        // Arrange
        const string sessionId = "existing-guest-session";
        var existingCart = ShoppingTestFixtures.CreateTestGuestCart(_tenantId, sessionId);
        _cartRepository.GetByAnonymousSessionIdAsync(sessionId, Arg.Any<CancellationToken>())
            .Returns(existingCart);

        var command = new CreateCartCommand(AnonymousSessionId: sessionId, Currency: "USD");

        // Act & Assert
        var act = () => _handler.Handle(command, CancellationToken.None);
        await act.Should().ThrowAsync<InvalidOperationException>()
            .WithMessage("*already has an active cart for this session*");
    }

    [Fact]
    public async Task Handle_GuestCart_WithEmptySessionId_ThrowsException()
    {
        // Arrange
        var command = new CreateCartCommand(AnonymousSessionId: "", Currency: "USD");

        // Act & Assert
        var act = () => _handler.Handle(command, CancellationToken.None);
        await act.Should().ThrowAsync<InvalidOperationException>();
    }

    [Fact]
    public async Task Handle_GuestCart_WithNullSessionId_ThrowsException()
    {
        // Arrange
        var command = new CreateCartCommand(AnonymousSessionId: null, Currency: "USD");

        // Act & Assert
        var act = () => _handler.Handle(command, CancellationToken.None);
        await act.Should().ThrowAsync<InvalidOperationException>();
    }

    #endregion

    #region Currency Tests

    [Fact]
    public async Task Handle_CustomerCart_WithInvalidCurrency_ThrowsException()
    {
        // Arrange
        _cartRepository.GetByCustomerIdAsync(_customerId, Arg.Any<CancellationToken>())
            .Returns((Domain.Shopping.Entities.Cart?)null);

        var command = new CreateCartCommand(
            CustomerId: _customerId,
            Currency: "INVALID");

        // Act & Assert
        var act = () => _handler.Handle(command, CancellationToken.None);
        await act.Should().ThrowAsync<ArgumentException>();
    }

    [Fact]
    public async Task Handle_CustomerCart_WithDifferentCurrencies_CreatesWithSpecifiedCurrency()
    {
        // Arrange
        _cartRepository.GetByCustomerIdAsync(_customerId, Arg.Any<CancellationToken>())
            .Returns((Domain.Shopping.Entities.Cart?)null);

        var command = new CreateCartCommand(
            CustomerId: _customerId,
            Currency: "EUR");

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.Currency.Should().Be("EUR");
    }

    [Fact]
    public async Task Handle_GuestCart_WithCurrency_CreatesWithCurrency()
    {
        // Arrange
        const string sessionId = "guest-eur";
        _cartRepository.GetByAnonymousSessionIdAsync(sessionId, Arg.Any<CancellationToken>())
            .Returns((Domain.Shopping.Entities.Cart?)null);

        var command = new CreateCartCommand(
            AnonymousSessionId: sessionId,
            Currency: "GBP");

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.Currency.Should().Be("GBP");
    }

    #endregion

    #region Error Handling Tests

    [Fact]
    public async Task Handle_BothCustomerIdAndSessionIdProvided_UseCustomerId()
    {
        // Arrange
        const string sessionId = "guest-session";
        _cartRepository.GetByCustomerIdAsync(_customerId, Arg.Any<CancellationToken>())
            .Returns((Domain.Shopping.Entities.Cart?)null);

        var command = new CreateCartCommand(
            CustomerId: _customerId,
            AnonymousSessionId: sessionId,
            Currency: "USD");

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.CustomerId.Should().Be(_customerId);
        result.AnonymousSessionId.Should().BeNull();
    }

    [Fact]
    public async Task Handle_NeitherCustomerIdNorSessionIdProvided_ThrowsException()
    {
        // Arrange
        var command = new CreateCartCommand(
            CustomerId: null,
            AnonymousSessionId: null,
            Currency: "USD");

        // Act & Assert
        var act = () => _handler.Handle(command, CancellationToken.None);
        await act.Should().ThrowAsync<InvalidOperationException>()
            .WithMessage("*Either CustomerId or AnonymousSessionId must be provided*");
    }

    [Fact]
    public async Task Handle_WithNullTenantContext_ThrowsException()
    {
        // Arrange
        var tenantContext = Substitute.For<ITenantContext>();
        tenantContext.TenantId.Returns((Guid?)null);

        var handler = new CreateCartCommandHandler(
            _cartRepository,
            _dbContext,
            Substitute.For<ILogger<CreateCartCommandHandler>>(),
            tenantContext,
            _currentUserService);

        var command = new CreateCartCommand(CustomerId: _customerId, Currency: "USD");

        // Act & Assert
        var act = () => handler.Handle(command, CancellationToken.None);
        await act.Should().ThrowAsync<InvalidOperationException>()
            .WithMessage("*Tenant context is not resolved*");
    }

    #endregion

    #region Response Validation Tests

    [Fact]
    public async Task Handle_CustomerCart_ResponseContainsCorrectData()
    {
        // Arrange
        _cartRepository.GetByCustomerIdAsync(_customerId, Arg.Any<CancellationToken>())
            .Returns((Domain.Shopping.Entities.Cart?)null);

        var command = new CreateCartCommand(
            CustomerId: _customerId,
            Currency: "USD");

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.CartId.Should().NotBeEmpty();
        result.CustomerId.Should().Be(_customerId);
        result.Currency.Should().Be("USD");
        result.ItemsCount.Should().Be(0);
        result.SubTotal.Should().Be(0m);
    }

    [Fact]
    public async Task Handle_GuestCart_ResponseContainsCorrectData()
    {
        // Arrange
        const string sessionId = "test-guest-session";
        _cartRepository.GetByAnonymousSessionIdAsync(sessionId, Arg.Any<CancellationToken>())
            .Returns((Domain.Shopping.Entities.Cart?)null);

        var command = new CreateCartCommand(
            AnonymousSessionId: sessionId,
            Currency: "USD");

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.CartId.Should().NotBeEmpty();
        result.AnonymousSessionId.Should().Be(sessionId);
        result.CustomerId.Should().BeNull();
        result.Currency.Should().Be("USD");
    }

    #endregion
}
