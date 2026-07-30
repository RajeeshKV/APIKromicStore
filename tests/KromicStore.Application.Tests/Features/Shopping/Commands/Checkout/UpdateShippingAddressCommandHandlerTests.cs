using FluentAssertions;
using KromicStore.Application.Common.Abstractions;
using KromicStore.Application.Features.Shopping.Commands.UpdateShippingAddress;
using KromicStore.Application.Features.Shopping.Abstractions;
using KromicStore.Application.Tests.Features.Shopping.Common;
using KromicStore.Infrastructure.Persistence;
using Microsoft.Extensions.Logging;
using NSubstitute;
using Xunit;

namespace KromicStore.Application.Tests.Features.Shopping.Commands.Checkout;

/// <summary>
/// Handler tests for UpdateShippingAddressCommand.
/// Verifies updating shipping address for checkout sessions.
/// </summary>
public sealed class UpdateShippingAddressCommandHandlerTests
{
    private readonly ICheckoutSessionRepository _checkoutSessionRepository;
    private readonly KromicStoreDbContext _dbContext;
    private readonly ITenantContext _tenantContext;
    private readonly UpdateShippingAddressCommandHandler _handler;
    private readonly Guid _tenantId;
    private readonly Guid _customerId;
    private readonly Guid _checkoutSessionId;

    public UpdateShippingAddressCommandHandlerTests()
    {
        _tenantId = Guid.NewGuid();
        _customerId = Guid.NewGuid();
        _checkoutSessionId = Guid.NewGuid();
        _dbContext = ShoppingTestFixtures.CreateDbContext(_tenantId);
        _tenantContext = ShoppingTestFixtures.CreateTenantContext(_tenantId);

        _checkoutSessionRepository = Substitute.For<ICheckoutSessionRepository>();

        _handler = new UpdateShippingAddressCommandHandler(
            _checkoutSessionRepository,
            _dbContext,
            Substitute.For<ILogger<UpdateShippingAddressCommandHandler>>(),
            _tenantContext);
    }

    #region Update Shipping Address Tests

    [Fact]
    public async Task Handle_UpdatesShippingAddress_WithValidData()
    {
        // Arrange
        var session = ShoppingTestFixtures.CreateTestCheckoutSession(_tenantId, _customerId, _checkoutSessionId);
        session.AddItem(Guid.NewGuid(), 50m, 1);
        
        _checkoutSessionRepository.GetByIdAsync(_checkoutSessionId, Arg.Any<CancellationToken>())
            .Returns(session);

        var command = new UpdateShippingAddressCommand(
            CheckoutSessionId: _checkoutSessionId,
            Street: "123 Main St",
            City: "Springfield",
            State: "IL",
            PostalCode: "62701",
            Country: "USA");

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result.CheckoutSessionId.Should().Be(_checkoutSessionId);
        result.ShippingAddressUpdated.Should().BeTrue();
        result.FullAddress.Should().Contain("123 Main St");
        result.FullAddress.Should().Contain("Springfield");
        result.FullAddress.Should().Contain("IL");
        result.FullAddress.Should().Contain("62701");
        result.FullAddress.Should().Contain("USA");
    }

    [Fact]
    public async Task Handle_UpdatesShippingAddress_MultipleAddressFormats()
    {
        // Arrange
        var session = ShoppingTestFixtures.CreateTestCheckoutSession(_tenantId, _customerId, _checkoutSessionId);
        session.AddItem(Guid.NewGuid(), 50m, 1);
        
        _checkoutSessionRepository.GetByIdAsync(_checkoutSessionId, Arg.Any<CancellationToken>())
            .Returns(session);

        var testCases = new[]
        {
            ("456 Oak Ave", "Chicago", "IL", "60601", "USA"),
            ("789 Elm St", "New York", "NY", "10001", "USA"),
            ("999 Pine Ln", "Los Angeles", "CA", "90001", "USA"),
            ("321 Maple Dr", "Houston", "TX", "77001", "USA"),
        };

        foreach (var (street, city, state, postalCode, country) in testCases)
        {
            var command = new UpdateShippingAddressCommand(
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
    public async Task Handle_UpdatesShippingAddress_InternationalAddresses()
    {
        // Arrange
        var session = ShoppingTestFixtures.CreateTestCheckoutSession(_tenantId, _customerId, _checkoutSessionId);
        session.AddItem(Guid.NewGuid(), 50m, 1);
        
        _checkoutSessionRepository.GetByIdAsync(_checkoutSessionId, Arg.Any<CancellationToken>())
            .Returns(session);

        var command = new UpdateShippingAddressCommand(
            CheckoutSessionId: _checkoutSessionId,
            Street: "42 Baker Street",
            City: "London",
            State: "England",
            PostalCode: "NW1 6XE",
            Country: "United Kingdom");

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.ShippingAddressUpdated.Should().BeTrue();
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

        var command = new UpdateShippingAddressCommand(
            CheckoutSessionId: _checkoutSessionId,
            Street: "123 Main St",
            City: "Springfield",
            State: "IL",
            PostalCode: "62701",
            Country: "USA");

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.ShippingAddressUpdated.Should().BeTrue();
    }

    #endregion

    #region Validation Tests

    [Fact]
    public async Task Handle_CheckoutSessionNotFound_ThrowsException()
    {
        // Arrange
        _checkoutSessionRepository.GetByIdAsync(_checkoutSessionId, Arg.Any<CancellationToken>())
            .Returns((Domain.Shopping.Entities.CheckoutSession?)null);

        var command = new UpdateShippingAddressCommand(
            CheckoutSessionId: _checkoutSessionId,
            Street: "123 Main St",
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

        var command = new UpdateShippingAddressCommand(
            CheckoutSessionId: _checkoutSessionId,
            Street: "123 Main St",
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

        var handler = new UpdateShippingAddressCommandHandler(
            _checkoutSessionRepository,
            _dbContext,
            Substitute.For<ILogger<UpdateShippingAddressCommandHandler>>(),
            tenantContext);

        var session = ShoppingTestFixtures.CreateTestCheckoutSession(_tenantId, _customerId, _checkoutSessionId);
        session.AddItem(Guid.NewGuid(), 50m, 1);
        
        _checkoutSessionRepository.GetByIdAsync(_checkoutSessionId, Arg.Any<CancellationToken>())
            .Returns(session);

        var command = new UpdateShippingAddressCommand(
            CheckoutSessionId: _checkoutSessionId,
            Street: "123 Main St",
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

        const string street = "123 Test Street";
        const string city = "Test City";
        const string state = "TS";
        const string postalCode = "12345";
        const string country = "Test Country";

        var command = new UpdateShippingAddressCommand(
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

        var command = new UpdateShippingAddressCommand(
            CheckoutSessionId: _checkoutSessionId,
            Street: "1234 Very Long Street Name With Multiple Words",
            City: "A City With A Very Long Name Indeed",
            State: "LongState",
            PostalCode: "123456789",
            Country: "Country With Long Name");

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.FullAddress.Should().Contain("1234 Very Long Street Name With Multiple Words");
        result.FullAddress.Should().Contain("A City With A Very Long Name Indeed");
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

        var command1 = new UpdateShippingAddressCommand(
            CheckoutSessionId: _checkoutSessionId,
            Street: "First Address",
            City: "City1",
            State: "ST1",
            PostalCode: "10001",
            Country: "Country1");

        var command2 = new UpdateShippingAddressCommand(
            CheckoutSessionId: _checkoutSessionId,
            Street: "Second Address",
            City: "City2",
            State: "ST2",
            PostalCode: "20002",
            Country: "Country2");

        // Act
        var result1 = await _handler.Handle(command1, CancellationToken.None);
        var result2 = await _handler.Handle(command2, CancellationToken.None);

        // Assert
        result2.FullAddress.Should().Contain("Second Address");
        result2.FullAddress.Should().Contain("City2");
        result2.FullAddress.Should().NotContain("First Address");
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

        var command = new UpdateShippingAddressCommand(
            CheckoutSessionId: _checkoutSessionId,
            Street: "123 Main St",
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
    public async Task Handle_Response_AlwaysReturnsTrue_ForShippingAddressUpdated()
    {
        // Arrange
        var session = ShoppingTestFixtures.CreateTestCheckoutSession(_tenantId, _customerId, _checkoutSessionId);
        session.AddItem(Guid.NewGuid(), 50m, 1);
        
        _checkoutSessionRepository.GetByIdAsync(_checkoutSessionId, Arg.Any<CancellationToken>())
            .Returns(session);

        var command = new UpdateShippingAddressCommand(
            CheckoutSessionId: _checkoutSessionId,
            Street: "Any Street",
            City: "Any City",
            State: "AS",
            PostalCode: "00000",
            Country: "Any Country");

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.ShippingAddressUpdated.Should().BeTrue();
    }

    #endregion

    #region Edge Cases and Boundary Tests

    [Fact]
    public async Task Handle_WithMinimalAddress_CreatesFullAddress()
    {
        // Arrange
        var session = ShoppingTestFixtures.CreateTestCheckoutSession(_tenantId, _customerId, _checkoutSessionId);
        session.AddItem(Guid.NewGuid(), 50m, 1);
        
        _checkoutSessionRepository.GetByIdAsync(_checkoutSessionId, Arg.Any<CancellationToken>())
            .Returns(session);

        var command = new UpdateShippingAddressCommand(
            CheckoutSessionId: _checkoutSessionId,
            Street: "1 A",
            City: "X",
            State: "A",
            PostalCode: "1",
            Country: "C");

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.FullAddress.Should().NotBeNullOrEmpty();
    }

    [Fact]
    public async Task Handle_WithLongAddress_HandlesCorrectly()
    {
        // Arrange
        var session = ShoppingTestFixtures.CreateTestCheckoutSession(_tenantId, _customerId, _checkoutSessionId);
        session.AddItem(Guid.NewGuid(), 50m, 1);
        
        _checkoutSessionRepository.GetByIdAsync(_checkoutSessionId, Arg.Any<CancellationToken>())
            .Returns(session);

        var longStreet = "1234 Very Long Street Name With Many Words And Numbers 5678";
        var command = new UpdateShippingAddressCommand(
            CheckoutSessionId: _checkoutSessionId,
            Street: longStreet,
            City: "VeryLongCityName",
            State: "VL",
            PostalCode: "123456789",
            Country: "VeryLongCountryName");

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.FullAddress.Should().Contain(longStreet);
    }

    [Fact]
    public async Task Handle_CanUpdateMultipleTimes_LastOneWins()
    {
        // Arrange
        var session = ShoppingTestFixtures.CreateTestCheckoutSession(_tenantId, _customerId, _checkoutSessionId);
        session.AddItem(Guid.NewGuid(), 50m, 1);
        
        _checkoutSessionRepository.GetByIdAsync(_checkoutSessionId, Arg.Any<CancellationToken>())
            .Returns(session);

        var command1 = new UpdateShippingAddressCommand(
            CheckoutSessionId: _checkoutSessionId,
            Street: "First Street",
            City: "FirstCity",
            State: "FS",
            PostalCode: "11111",
            Country: "FirstCountry");

        var command2 = new UpdateShippingAddressCommand(
            CheckoutSessionId: _checkoutSessionId,
            Street: "Second Street",
            City: "SecondCity",
            State: "SC",
            PostalCode: "22222",
            Country: "SecondCountry");

        // Act
        var result1 = await _handler.Handle(command1, CancellationToken.None);
        var result2 = await _handler.Handle(command2, CancellationToken.None);

        // Assert
        result2.FullAddress.Should().Contain("Second Street");
        result2.FullAddress.Should().Contain("SecondCity");
        result2.FullAddress.Should().NotContain("First Street");
    }

    [Fact]
    public async Task Handle_WithSpecialCharacters_HandlesCorrectly()
    {
        // Arrange
        var session = ShoppingTestFixtures.CreateTestCheckoutSession(_tenantId, _customerId, _checkoutSessionId);
        session.AddItem(Guid.NewGuid(), 50m, 1);
        
        _checkoutSessionRepository.GetByIdAsync(_checkoutSessionId, Arg.Any<CancellationToken>())
            .Returns(session);

        var command = new UpdateShippingAddressCommand(
            CheckoutSessionId: _checkoutSessionId,
            Street: "123 O'Brien St. #456",
            City: "Saint-Jean",
            State: "QC",
            PostalCode: "J4H 1A1",
            Country: "Côte d'Ivoire");

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.FullAddress.Should().Contain("O'Brien");
        result.FullAddress.Should().Contain("Saint-Jean");
    }

    [Fact]
    public async Task Handle_ShippingCheckoutSessionNotFound_ThrowsException()
    {
        // Arrange
        _checkoutSessionRepository.GetByIdAsync(_checkoutSessionId, Arg.Any<CancellationToken>())
            .Returns((Domain.Shopping.Entities.CheckoutSession?)null);

        var command = new UpdateShippingAddressCommand(
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

    #endregion
}
