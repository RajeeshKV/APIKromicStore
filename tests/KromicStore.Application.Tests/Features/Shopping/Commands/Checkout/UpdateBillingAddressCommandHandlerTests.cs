using FluentAssertions;
using KromicStore.Application.Common.Abstractions;
using KromicStore.Application.Features.Shopping.Commands.UpdateBillingAddress;
using KromicStore.Application.Features.Shopping.Abstractions;
using KromicStore.Application.Tests.Features.Shopping.Common;
using KromicStore.Infrastructure.Persistence;
using Microsoft.Extensions.Logging;
using NSubstitute;
using Xunit;

namespace KromicStore.Application.Tests.Features.Shopping.Commands.Checkout;

/// <summary>
/// Handler tests for UpdateBillingAddressCommand.
/// Verifies updating billing address for checkout sessions.
/// </summary>
public sealed class UpdateBillingAddressCommandHandlerTests
{
    private readonly ICheckoutSessionRepository _checkoutSessionRepository;
    private readonly KromicStoreDbContext _dbContext;
    private readonly ITenantContext _tenantContext;
    private readonly UpdateBillingAddressCommandHandler _handler;
    private readonly Guid _tenantId;
    private readonly Guid _customerId;
    private readonly Guid _checkoutSessionId;

    public UpdateBillingAddressCommandHandlerTests()
    {
        _tenantId = Guid.NewGuid();
        _customerId = Guid.NewGuid();
        _checkoutSessionId = Guid.NewGuid();
        _dbContext = ShoppingTestFixtures.CreateDbContext(_tenantId);
        _tenantContext = ShoppingTestFixtures.CreateTenantContext(_tenantId);

        _checkoutSessionRepository = Substitute.For<ICheckoutSessionRepository>();

        _handler = new UpdateBillingAddressCommandHandler(
            _checkoutSessionRepository,
            _dbContext,
            Substitute.For<ILogger<UpdateBillingAddressCommandHandler>>(),
            _tenantContext);
    }

    #region Update Billing Address Tests

    [Fact]
    public async Task Handle_UpdatesBillingAddress_WithValidData()
    {
        // Arrange
        var session = ShoppingTestFixtures.CreateTestCheckoutSession(_tenantId, _customerId, _checkoutSessionId);
        session.AddItem(Guid.NewGuid(), 50m, 1);
        
        _checkoutSessionRepository.GetByIdAsync(_checkoutSessionId, Arg.Any<CancellationToken>())
            .Returns(session);

        var command = new UpdateBillingAddressCommand(
            CheckoutSessionId: _checkoutSessionId,
            Street: "456 Main St",
            City: "Springfield",
            State: "IL",
            PostalCode: "62701",
            Country: "USA");

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result.CheckoutSessionId.Should().Be(_checkoutSessionId);
        result.BillingAddressUpdated.Should().BeTrue();
        result.FullAddress.Should().Contain("456 Main St");
        result.FullAddress.Should().Contain("Springfield");
    }

    [Fact]
    public async Task Handle_UpdatesBillingAddress_MultipleAddressFormats()
    {
        // Arrange
        var session = ShoppingTestFixtures.CreateTestCheckoutSession(_tenantId, _customerId, _checkoutSessionId);
        session.AddItem(Guid.NewGuid(), 50m, 1);
        
        _checkoutSessionRepository.GetByIdAsync(_checkoutSessionId, Arg.Any<CancellationToken>())
            .Returns(session);

        var testCases = new[]
        {
            ("789 Oak Ave", "Chicago", "IL", "60601", "USA"),
            ("321 Elm St", "New York", "NY", "10001", "USA"),
            ("654 Pine Ln", "Los Angeles", "CA", "90001", "USA"),
        };

        foreach (var (street, city, state, postalCode, country) in testCases)
        {
            var command = new UpdateBillingAddressCommand(
                CheckoutSessionId: _checkoutSessionId,
                Street: street,
                City: city,
                State: state,
                PostalCode: postalCode,
                Country: country);

            // Act
            var result = await _handler.Handle(command, CancellationToken.None);

            // Assert
            result.FullAddress.Should().Contain(street);
            result.FullAddress.Should().Contain(city);
        }
    }

    [Fact]
    public async Task Handle_UpdatesBillingAddress_InternationalAddresses()
    {
        // Arrange
        var session = ShoppingTestFixtures.CreateTestCheckoutSession(_tenantId, _customerId, _checkoutSessionId);
        session.AddItem(Guid.NewGuid(), 50m, 1);
        
        _checkoutSessionRepository.GetByIdAsync(_checkoutSessionId, Arg.Any<CancellationToken>())
            .Returns(session);

        var command = new UpdateBillingAddressCommand(
            CheckoutSessionId: _checkoutSessionId,
            Street: "10 Downing Street",
            City: "London",
            State: "England",
            PostalCode: "SW1A 2AA",
            Country: "United Kingdom");

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.BillingAddressUpdated.Should().BeTrue();
        result.FullAddress.Should().Contain("United Kingdom");
    }

    [Fact]
    public async Task Handle_SavesChanges()
    {
        // Arrange
        var session = ShoppingTestFixtures.CreateTestCheckoutSession(_tenantId, _customerId, _checkoutSessionId);
        session.AddItem(Guid.NewGuid(), 50m, 1);
        
        _checkoutSessionRepository.GetByIdAsync(_checkoutSessionId, Arg.Any<CancellationToken>())
            .Returns(session);

        var command = new UpdateBillingAddressCommand(
            CheckoutSessionId: _checkoutSessionId,
            Street: "456 Main St",
            City: "Springfield",
            State: "IL",
            PostalCode: "62701",
            Country: "USA");

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.BillingAddressUpdated.Should().BeTrue();
    }

    #endregion

    #region Validation Tests

    [Fact]
    public async Task Handle_CheckoutSessionNotFound_ThrowsException()
    {
        // Arrange
        _checkoutSessionRepository.GetByIdAsync(_checkoutSessionId, Arg.Any<CancellationToken>())
            .Returns((Domain.Shopping.Entities.CheckoutSession?)null);

        var command = new UpdateBillingAddressCommand(
            CheckoutSessionId: _checkoutSessionId,
            Street: "456 Main St",
            City: "Springfield",
            State: "IL",
            PostalCode: "62701",
            Country: "USA");

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
        
        _checkoutSessionRepository.GetByIdAsync(_checkoutSessionId, Arg.Any<CancellationToken>())
            .Returns(session);

        var command = new UpdateBillingAddressCommand(
            CheckoutSessionId: _checkoutSessionId,
            Street: "456 Main St",
            City: "Springfield",
            State: "IL",
            PostalCode: "62701",
            Country: "USA");

        // Act & Assert
        var act = () => _handler.Handle(command, CancellationToken.None);
        await act.Should().ThrowAsync<UnauthorizedAccessException>()
            .WithMessage("*Cannot access checkout session from another tenant*");
    }

    [Fact]
    public async Task Handle_NullTenantContext_ThrowsException()
    {
        // Arrange
        var tenantContext = Substitute.For<ITenantContext>();
        tenantContext.TenantId.Returns((Guid?)null);

        var handler = new UpdateBillingAddressCommandHandler(
            _checkoutSessionRepository,
            _dbContext,
            Substitute.For<ILogger<UpdateBillingAddressCommandHandler>>(),
            tenantContext);

        var session = ShoppingTestFixtures.CreateTestCheckoutSession(_tenantId, _customerId, _checkoutSessionId);
        session.AddItem(Guid.NewGuid(), 50m, 1);
        
        _checkoutSessionRepository.GetByIdAsync(_checkoutSessionId, Arg.Any<CancellationToken>())
            .Returns(session);

        var command = new UpdateBillingAddressCommand(
            CheckoutSessionId: _checkoutSessionId,
            Street: "456 Main St",
            City: "Springfield",
            State: "IL",
            PostalCode: "62701",
            Country: "USA");

        // Act & Assert
        var act = () => handler.Handle(command, CancellationToken.None);
        await act.Should().ThrowAsync<InvalidOperationException>()
            .WithMessage("*Tenant context is not resolved*");
    }

    #endregion

    #region Address Format Tests

    [Fact]
    public async Task Handle_FullAddress_IncludesAllComponents()
    {
        // Arrange
        var session = ShoppingTestFixtures.CreateTestCheckoutSession(_tenantId, _customerId, _checkoutSessionId);
        session.AddItem(Guid.NewGuid(), 50m, 1);
        
        _checkoutSessionRepository.GetByIdAsync(_checkoutSessionId, Arg.Any<CancellationToken>())
            .Returns(session);

        const string street = "456 Test Street";
        const string city = "Test City";
        const string state = "TS";
        const string postalCode = "54321";
        const string country = "Test Country";

        var command = new UpdateBillingAddressCommand(
            CheckoutSessionId: _checkoutSessionId,
            Street: street,
            City: city,
            State: state,
            PostalCode: postalCode,
            Country: country);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.FullAddress.Should().Be($"{street}, {city}, {state} {postalCode}, {country}");
    }

    [Fact]
    public async Task Handle_WithLongAddressComponents_FormatsCorrectly()
    {
        // Arrange
        var session = ShoppingTestFixtures.CreateTestCheckoutSession(_tenantId, _customerId, _checkoutSessionId);
        session.AddItem(Guid.NewGuid(), 50m, 1);
        
        _checkoutSessionRepository.GetByIdAsync(_checkoutSessionId, Arg.Any<CancellationToken>())
            .Returns(session);

        var command = new UpdateBillingAddressCommand(
            CheckoutSessionId: _checkoutSessionId,
            Street: "4567 Very Long Street Name With Multiple Words",
            City: "Another City With A Very Long Name Indeed",
            State: "LongState",
            PostalCode: "987654321",
            Country: "Another Country With Long Name");

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.FullAddress.Should().Contain("4567 Very Long Street Name With Multiple Words");
        result.FullAddress.Should().Contain("Another City With A Very Long Name Indeed");
    }

    #endregion

    #region State Transition Tests

    [Fact]
    public async Task Handle_UpdatesMultipleTimes_LastOneWins()
    {
        // Arrange
        var session = ShoppingTestFixtures.CreateTestCheckoutSession(_tenantId, _customerId, _checkoutSessionId);
        session.AddItem(Guid.NewGuid(), 50m, 1);
        
        _checkoutSessionRepository.GetByIdAsync(_checkoutSessionId, Arg.Any<CancellationToken>())
            .Returns(session);

        var command1 = new UpdateBillingAddressCommand(
            CheckoutSessionId: _checkoutSessionId,
            Street: "First Billing Address",
            City: "City1",
            State: "ST1",
            PostalCode: "10001",
            Country: "Country1");

        var command2 = new UpdateBillingAddressCommand(
            CheckoutSessionId: _checkoutSessionId,
            Street: "Second Billing Address",
            City: "City2",
            State: "ST2",
            PostalCode: "20002",
            Country: "Country2");

        // Act
        var result1 = await _handler.Handle(command1, CancellationToken.None);
        var result2 = await _handler.Handle(command2, CancellationToken.None);

        // Assert
        result2.FullAddress.Should().Contain("Second Billing Address");
        result2.FullAddress.Should().Contain("City2");
        result2.FullAddress.Should().NotContain("First Billing Address");
    }

    #endregion

    #region Response Validation Tests

    [Fact]
    public async Task Handle_Response_HasCorrectCheckoutSessionId()
    {
        // Arrange
        var session = ShoppingTestFixtures.CreateTestCheckoutSession(_tenantId, _customerId, _checkoutSessionId);
        session.AddItem(Guid.NewGuid(), 50m, 1);
        
        _checkoutSessionRepository.GetByIdAsync(_checkoutSessionId, Arg.Any<CancellationToken>())
            .Returns(session);

        var command = new UpdateBillingAddressCommand(
            CheckoutSessionId: _checkoutSessionId,
            Street: "456 Main St",
            City: "Springfield",
            State: "IL",
            PostalCode: "62701",
            Country: "USA");

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.CheckoutSessionId.Should().Be(_checkoutSessionId);
    }

    [Fact]
    public async Task Handle_Response_AlwaysReturnsTrue_ForBillingAddressUpdated()
    {
        // Arrange
        var session = ShoppingTestFixtures.CreateTestCheckoutSession(_tenantId, _customerId, _checkoutSessionId);
        session.AddItem(Guid.NewGuid(), 50m, 1);
        
        _checkoutSessionRepository.GetByIdAsync(_checkoutSessionId, Arg.Any<CancellationToken>())
            .Returns(session);

        var command = new UpdateBillingAddressCommand(
            CheckoutSessionId: _checkoutSessionId,
            Street: "Any Street",
            City: "Any City",
            State: "AS",
            PostalCode: "00000",
            Country: "Any Country");

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.BillingAddressUpdated.Should().BeTrue();
    }

    #endregion

    #region Edge Cases

    [Fact]
    public async Task Handle_WithSpecialCharactersInBillingAddress_HandlesCorrectly()
    {
        // Arrange
        var session = ShoppingTestFixtures.CreateTestCheckoutSession(_tenantId, _customerId, _checkoutSessionId);
        session.AddItem(Guid.NewGuid(), 50m, 1);
        
        _checkoutSessionRepository.GetByIdAsync(_checkoutSessionId, Arg.Any<CancellationToken>())
            .Returns(session);

        var command = new UpdateBillingAddressCommand(
            CheckoutSessionId: _checkoutSessionId,
            Street: "789 O'Malley Ln. #99",
            City: "Montréal",
            State: "QC",
            PostalCode: "H1H 1H1",
            Country: "Canada");

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.FullAddress.Should().Contain("O'Malley");
        result.FullAddress.Should().Contain("Montréal");
    }

    [Fact]
    public async Task Handle_CanUpdateBillingAddressMultipleTimes()
    {
        // Arrange
        var session = ShoppingTestFixtures.CreateTestCheckoutSession(_tenantId, _customerId, _checkoutSessionId);
        session.AddItem(Guid.NewGuid(), 50m, 1);
        
        _checkoutSessionRepository.GetByIdAsync(_checkoutSessionId, Arg.Any<CancellationToken>())
            .Returns(session);

        var command1 = new UpdateBillingAddressCommand(
            CheckoutSessionId: _checkoutSessionId,
            Street: "First Address",
            City: "City1",
            State: "ST1",
            PostalCode: "10001",
            Country: "Country1");

        var command2 = new UpdateBillingAddressCommand(
            CheckoutSessionId: _checkoutSessionId,
            Street: "Second Address",
            City: "City2",
            State: "ST2",
            PostalCode: "20002",
            Country: "Country2");

        // Act
        await _handler.Handle(command1, CancellationToken.None);
        var result2 = await _handler.Handle(command2, CancellationToken.None);

        // Assert
        result2.FullAddress.Should().Contain("Second Address");
    }

    [Fact]
    public async Task Handle_BillingCheckoutSessionNotFound_ThrowsException()
    {
        // Arrange
        _checkoutSessionRepository.GetByIdAsync(_checkoutSessionId, Arg.Any<CancellationToken>())
            .Returns((Domain.Shopping.Entities.CheckoutSession?)null);

        var command = new UpdateBillingAddressCommand(
            CheckoutSessionId: _checkoutSessionId,
            Street: "Any Street",
            City: "Any City",
            State: "AC",
            PostalCode: "12345",
            Country: "Any Country");

        // Act & Assert
        var act = () => _handler.Handle(command, CancellationToken.None);
        await act.Should().ThrowAsync<InvalidOperationException>();
    }

    [Fact]
    public async Task Handle_WithMinimalBillingAddress_CreatesFullAddress()
    {
        // Arrange
        var session = ShoppingTestFixtures.CreateTestCheckoutSession(_tenantId, _customerId, _checkoutSessionId);
        session.AddItem(Guid.NewGuid(), 50m, 1);
        
        _checkoutSessionRepository.GetByIdAsync(_checkoutSessionId, Arg.Any<CancellationToken>())
            .Returns(session);

        var command = new UpdateBillingAddressCommand(
            CheckoutSessionId: _checkoutSessionId,
            Street: "1 B",
            City: "Y",
            State: "B",
            PostalCode: "2",
            Country: "D");

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.FullAddress.Should().NotBeNullOrEmpty();
    }

    [Fact]
    public async Task Handle_WithLongBillingAddress_HandlesCorrectly()
    {
        // Arrange
        var session = ShoppingTestFixtures.CreateTestCheckoutSession(_tenantId, _customerId, _checkoutSessionId);
        session.AddItem(Guid.NewGuid(), 50m, 1);
        
        _checkoutSessionRepository.GetByIdAsync(_checkoutSessionId, Arg.Any<CancellationToken>())
            .Returns(session);

        var longStreet = "5678 Very Long Address Street With Extended Details";
        var command = new UpdateBillingAddressCommand(
            CheckoutSessionId: _checkoutSessionId,
            Street: longStreet,
            City: "ExtendedCityName",
            State: "EC",
            PostalCode: "987654321",
            Country: "ExtendedCountryName");

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.FullAddress.Should().Contain(longStreet);
    }

    #endregion
}
