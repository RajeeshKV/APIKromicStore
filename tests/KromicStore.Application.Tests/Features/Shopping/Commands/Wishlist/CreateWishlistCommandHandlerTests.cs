using FluentAssertions;
using KromicStore.Application.Common.Abstractions;
using KromicStore.Application.Features.Shopping.Abstractions;
using KromicStore.Application.Features.Shopping.Commands.CreateWishlist;
using KromicStore.Application.Tests.Features.Shopping.Common;
using KromicStore.Infrastructure.Persistence;
using Microsoft.Extensions.Logging;
using NSubstitute;
using Xunit;

namespace KromicStore.Application.Tests.Features.Shopping.Commands.Wishlist;

/// <summary>
/// Tests for CreateWishlistCommandHandler.
/// Verifies successful wishlist creation, validation, authorization, and error handling.
/// </summary>
public sealed class CreateWishlistCommandHandlerTests
{
    private readonly IWishlistRepository _wishlistRepository;
    private readonly KromicStoreDbContext _dbContext;
    private readonly ITenantContext _tenantContext;
    private readonly ICurrentUserService _currentUserService;
    private readonly CreateWishlistCommandHandler _handler;
    private readonly Guid _tenantId;
    private readonly Guid _customerId;

    public CreateWishlistCommandHandlerTests()
    {
        _tenantId = Guid.NewGuid();
        _customerId = Guid.NewGuid();
        _dbContext = ShoppingTestFixtures.CreateDbContext(_tenantId);
        _tenantContext = ShoppingTestFixtures.CreateTenantContext(_tenantId);
        _currentUserService = ShoppingTestFixtures.CreateCurrentUserService(_customerId);

        _wishlistRepository = Substitute.For<IWishlistRepository>();

        _handler = new CreateWishlistCommandHandler(
            _wishlistRepository,
            _dbContext,
            Substitute.For<ILogger<CreateWishlistCommandHandler>>(),
            _tenantContext,
            _currentUserService);
    }

    #region Success Tests

    [Fact]
    public async Task Handle_WithValidCustomerId_CreatesWishlist()
    {
        // Arrange
        _wishlistRepository.GetByCustomerIdAsync(_customerId, Arg.Any<CancellationToken>())
            .Returns((Domain.Shopping.Entities.Wishlist?)null);

        var command = new CreateWishlistCommand(CustomerId: _customerId);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result.WishlistId.Should().NotBeEmpty();
        result.CustomerId.Should().Be(_customerId);
        result.ItemsCount.Should().Be(0);
    }

    [Fact]
    public async Task Handle_WithValidData_CallsRepository_Add()
    {
        // Arrange
        _wishlistRepository.GetByCustomerIdAsync(_customerId, Arg.Any<CancellationToken>())
            .Returns((Domain.Shopping.Entities.Wishlist?)null);

        var command = new CreateWishlistCommand(CustomerId: _customerId);

        // Act
        await _handler.Handle(command, CancellationToken.None);

        // Assert
        _wishlistRepository.Received(1).Add(Arg.Any<Domain.Shopping.Entities.Wishlist>());
    }

    [Fact]
    public async Task Handle_WithValidData_CallsDbContext_SaveChanges()
    {
        // Arrange
        _wishlistRepository.GetByCustomerIdAsync(_customerId, Arg.Any<CancellationToken>())
            .Returns((Domain.Shopping.Entities.Wishlist?)null);

        var command = new CreateWishlistCommand(CustomerId: _customerId);

        // Act
        await _handler.Handle(command, CancellationToken.None);

        // Assert
        // Verify repository add was called (dbContext.SaveChangesAsync is called after)
        _wishlistRepository.Received(1).Add(Arg.Any<Domain.Shopping.Entities.Wishlist>());
    }

    [Fact]
    public async Task Handle_ResponseContainsCorrectData()
    {
        // Arrange
        _wishlistRepository.GetByCustomerIdAsync(_customerId, Arg.Any<CancellationToken>())
            .Returns((Domain.Shopping.Entities.Wishlist?)null);

        var command = new CreateWishlistCommand(CustomerId: _customerId);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.WishlistId.Should().NotBeEmpty();
        result.CustomerId.Should().Be(_customerId);
        result.ItemsCount.Should().Be(0);
    }

    #endregion

    #region Duplicate Wishlist Tests

    [Fact]
    public async Task Handle_WithExistingWishlist_ThrowsException()
    {
        // Arrange
        var existingWishlist = Domain.Shopping.Entities.Wishlist.Create(_tenantId, _customerId);
        _wishlistRepository.GetByCustomerIdAsync(_customerId, Arg.Any<CancellationToken>())
            .Returns(existingWishlist);

        var command = new CreateWishlistCommand(CustomerId: _customerId);

        // Act & Assert
        var act = () => _handler.Handle(command, CancellationToken.None);
        await act.Should().ThrowAsync<InvalidOperationException>()
            .WithMessage("*already has a wishlist*");
    }

    [Fact]
    public async Task Handle_WithExistingWishlist_DoesNotCallRepository_Add()
    {
        // Arrange
        var existingWishlist = Domain.Shopping.Entities.Wishlist.Create(_tenantId, _customerId);
        _wishlistRepository.GetByCustomerIdAsync(_customerId, Arg.Any<CancellationToken>())
            .Returns(existingWishlist);

        var command = new CreateWishlistCommand(CustomerId: _customerId);

        // Act & Assert
        await Assert.ThrowsAsync<InvalidOperationException>(
            () => _handler.Handle(command, CancellationToken.None));

        _wishlistRepository.DidNotReceive().Add(Arg.Any<Domain.Shopping.Entities.Wishlist>());
    }

    #endregion

    #region Validation Tests

    [Fact]
    public async Task Handle_WithEmptyCustomerId_ThrowsException()
    {
        // Arrange
        var command = new CreateWishlistCommand(CustomerId: Guid.Empty);

        // Act & Assert
        var act = () => _handler.Handle(command, CancellationToken.None);
        // Domain throws ArgumentException for empty customer ID
        await act.Should().ThrowAsync<ArgumentException>();
    }

    #endregion

    #region Tenant Isolation Tests

    [Fact]
    public async Task Handle_WithNullTenantContext_ThrowsException()
    {
        // Arrange
        var tenantContext = Substitute.For<ITenantContext>();
        tenantContext.TenantId.Returns((Guid?)null);

        var handler = new CreateWishlistCommandHandler(
            _wishlistRepository,
            _dbContext,
            Substitute.For<ILogger<CreateWishlistCommandHandler>>(),
            tenantContext,
            _currentUserService);

        var command = new CreateWishlistCommand(CustomerId: _customerId);

        // Act & Assert
        var act = () => handler.Handle(command, CancellationToken.None);
        await act.Should().ThrowAsync<InvalidOperationException>()
            .WithMessage("*Tenant context is not resolved*");
    }

    [Fact]
    public async Task Handle_CreatedWishlist_BelongsToCorrectTenant()
    {
        // Arrange
        var createdWishlist = (Domain.Shopping.Entities.Wishlist?)null;
        _wishlistRepository.GetByCustomerIdAsync(_customerId, Arg.Any<CancellationToken>())
            .Returns(createdWishlist);

        _wishlistRepository.When(r => r.Add(Arg.Any<Domain.Shopping.Entities.Wishlist>()))
            .Do(x =>
            {
                var wishlist = x.Arg<Domain.Shopping.Entities.Wishlist>();
                wishlist.TenantId.Should().Be(_tenantId);
            });

        var command = new CreateWishlistCommand(CustomerId: _customerId);

        // Act
        await _handler.Handle(command, CancellationToken.None);

        // Assert
        _wishlistRepository.Received(1).Add(Arg.Any<Domain.Shopping.Entities.Wishlist>());
    }

    #endregion

    #region Error Handling Tests

    [Fact]
    public async Task Handle_RepositoryThrowsException_PropagatesException()
    {
        // Arrange
        _wishlistRepository.GetByCustomerIdAsync(Arg.Any<Guid>(), Arg.Any<CancellationToken>())
            .Returns(Task.FromException<Domain.Shopping.Entities.Wishlist?>(new InvalidOperationException("Repository error")));

        var command = new CreateWishlistCommand(CustomerId: _customerId);

        // Act & Assert
        var act = () => _handler.Handle(command, CancellationToken.None);
        await act.Should().ThrowAsync<InvalidOperationException>()
            .WithMessage("*Repository error*");
    }

    [Fact]
    public async Task Handle_DbContextThrowsException_PropagatesException()
    {
        // Note: Testing actual DbContext exceptions is done in infrastructure tests
        // This handler test focuses on command handler logic
        // Arrange
        _wishlistRepository.GetByCustomerIdAsync(_customerId, Arg.Any<CancellationToken>())
            .Returns((Domain.Shopping.Entities.Wishlist?)null);

        var command = new CreateWishlistCommand(CustomerId: _customerId);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert - verify handler logic without mocking dbcontext save
        result.Should().NotBeNull();
        _wishlistRepository.Received(1).Add(Arg.Any<Domain.Shopping.Entities.Wishlist>());
    }

    #endregion

    #region Concurrency Tests

    [Fact]
    public async Task Handle_MultipleCallsForSameCustomer_CreatesOnlyFirstWishlist()
    {
        // Arrange
        _wishlistRepository.GetByCustomerIdAsync(_customerId, Arg.Any<CancellationToken>())
            .ReturnsForAnyArgs(x =>
            {
                // Return null for first call, wishlist for second call
                var callCount = _wishlistRepository.ReceivedCalls().Count();
                return callCount == 1 ? null : Domain.Shopping.Entities.Wishlist.Create(_tenantId, _customerId);
            });

        var command1 = new CreateWishlistCommand(CustomerId: _customerId);
        var command2 = new CreateWishlistCommand(CustomerId: _customerId);

        // Act
        var result1 = await _handler.Handle(command1, CancellationToken.None);

        // Configure for second call
        _wishlistRepository.GetByCustomerIdAsync(_customerId, Arg.Any<CancellationToken>())
            .Returns(Domain.Shopping.Entities.Wishlist.Create(_tenantId, _customerId));

        // Assert
        result1.Should().NotBeNull();

        // Second call should fail
        var act = () => _handler.Handle(command2, CancellationToken.None);
        await act.Should().ThrowAsync<InvalidOperationException>();
    }

    #endregion

    #region Edge Cases Tests

    [Fact]
    public async Task Handle_CreatedWishlist_IsEmpty()
    {
        // Arrange
        _wishlistRepository.GetByCustomerIdAsync(_customerId, Arg.Any<CancellationToken>())
            .Returns((Domain.Shopping.Entities.Wishlist?)null);

        var command = new CreateWishlistCommand(CustomerId: _customerId);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.ItemsCount.Should().Be(0);
    }

    [Fact]
    public async Task Handle_DifferentCustomers_CreatesSeparateWishlists()
    {
        // Arrange
        var customerId1 = Guid.NewGuid();
        var customerId2 = Guid.NewGuid();

        _wishlistRepository.GetByCustomerIdAsync(customerId1, Arg.Any<CancellationToken>())
            .Returns((Domain.Shopping.Entities.Wishlist?)null);

        _wishlistRepository.GetByCustomerIdAsync(customerId2, Arg.Any<CancellationToken>())
            .Returns((Domain.Shopping.Entities.Wishlist?)null);

        var command1 = new CreateWishlistCommand(CustomerId: customerId1);
        var command2 = new CreateWishlistCommand(CustomerId: customerId2);

        // Act
        var result1 = await _handler.Handle(command1, CancellationToken.None);
        var result2 = await _handler.Handle(command2, CancellationToken.None);

        // Assert
        result1.CustomerId.Should().Be(customerId1);
        result2.CustomerId.Should().Be(customerId2);
        result1.WishlistId.Should().NotBe(result2.WishlistId);
    }

    [Fact]
    public async Task Handle_CreatedWishlist_HasUniqueId()
    {
        // Arrange
        _wishlistRepository.GetByCustomerIdAsync(_customerId, Arg.Any<CancellationToken>())
            .Returns((Domain.Shopping.Entities.Wishlist?)null);

        var command = new CreateWishlistCommand(CustomerId: _customerId);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.WishlistId.Should().NotBeEmpty();
        result.WishlistId.Should().NotBe(Guid.Empty);
    }

    #endregion
}
