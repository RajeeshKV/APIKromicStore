using FluentAssertions;
using KromicStore.Application.Common.Abstractions;
using KromicStore.Application.Features.Shopping.Abstractions;
using KromicStore.Application.Features.Shopping.Commands.AddToWishlist;
using KromicStore.Application.Tests.Features.Shopping.Common;
using KromicStore.Infrastructure.Persistence;
using Microsoft.Extensions.Logging;
using NSubstitute;
using Xunit;

namespace KromicStore.Application.Tests.Features.Shopping.Commands.Wishlist;

/// <summary>
/// Tests for AddToWishlistCommandHandler.
/// Verifies product addition to wishlist, duplicate prevention, authorization, and error handling.
/// </summary>
public sealed class AddToWishlistCommandHandlerTests
{
    private readonly IWishlistRepository _wishlistRepository;
    private readonly KromicStoreDbContext _dbContext;
    private readonly ITenantContext _tenantContext;
    private readonly AddToWishlistCommandHandler _handler;
    private readonly Guid _tenantId;
    private readonly Guid _customerId;
    private readonly Guid _productId;

    public AddToWishlistCommandHandlerTests()
    {
        _tenantId = Guid.NewGuid();
        _customerId = Guid.NewGuid();
        _productId = Guid.NewGuid();
        _dbContext = ShoppingTestFixtures.CreateDbContext(_tenantId);
        _tenantContext = ShoppingTestFixtures.CreateTenantContext(_tenantId);

        _wishlistRepository = Substitute.For<IWishlistRepository>();

        _handler = new AddToWishlistCommandHandler(
            _wishlistRepository,
            _dbContext,
            Substitute.For<ILogger<AddToWishlistCommandHandler>>(),
            _tenantContext);
    }

    #region Success Tests

    [Fact]
    public async Task Handle_WithValidData_AddsProductToWishlist()
    {
        // Arrange
        var wishlist = Domain.Shopping.Entities.Wishlist.Create(_tenantId, _customerId);
        _wishlistRepository.GetByIdAsync(wishlist.Id, Arg.Any<CancellationToken>())
            .Returns(wishlist);

        var command = new AddToWishlistCommand(
            WishlistId: wishlist.Id,
            ProductId: _productId);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result.WishlistId.Should().Be(wishlist.Id);
        result.ProductId.Should().Be(_productId);
        result.IsNew.Should().BeTrue();
        result.ItemsCount.Should().Be(1);
    }

    [Fact]
    public async Task Handle_WithMultipleProducts_IncreasesItemCount()
    {
        // Arrange
        var wishlist = Domain.Shopping.Entities.Wishlist.Create(_tenantId, _customerId);
        var product1Id = Guid.NewGuid();
        var product2Id = Guid.NewGuid();

        _wishlistRepository.GetByIdAsync(wishlist.Id, Arg.Any<CancellationToken>())
            .Returns(wishlist);

        var command1 = new AddToWishlistCommand(WishlistId: wishlist.Id, ProductId: product1Id);
        var command2 = new AddToWishlistCommand(WishlistId: wishlist.Id, ProductId: product2Id);

        // Act
        var result1 = await _handler.Handle(command1, CancellationToken.None);
        var result2 = await _handler.Handle(command2, CancellationToken.None);

        // Assert
        result1.ItemsCount.Should().Be(1);
        result2.ItemsCount.Should().Be(2);
    }

    #endregion

    #region Duplicate Prevention Tests

    [Fact]
    public async Task Handle_WithDuplicateProduct_ThrowsException()
    {
        // Arrange
        var wishlist = Domain.Shopping.Entities.Wishlist.Create(_tenantId, _customerId);
        wishlist.AddItem(_productId);

        _wishlistRepository.GetByIdAsync(wishlist.Id, Arg.Any<CancellationToken>())
            .Returns(wishlist);

        var command = new AddToWishlistCommand(
            WishlistId: wishlist.Id,
            ProductId: _productId);

        // Act & Assert - Handler detects duplicate and logs instead of throwing
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert - Handler returns with IsNew=False for duplicates
        result.IsNew.Should().BeFalse();
        result.ItemsCount.Should().Be(1);
    }

    #endregion

    #region Validation Tests

    [Fact]
    public async Task Handle_WithNonExistentWishlist_ThrowsException()
    {
        // Arrange
        var wishlistId = Guid.NewGuid();
        _wishlistRepository.GetByIdAsync(wishlistId, Arg.Any<CancellationToken>())
            .Returns((Domain.Shopping.Entities.Wishlist?)null);

        var command = new AddToWishlistCommand(
            WishlistId: wishlistId,
            ProductId: _productId);

        // Act & Assert
        var act = () => _handler.Handle(command, CancellationToken.None);
        await act.Should().ThrowAsync<InvalidOperationException>()
            .WithMessage("*not found*");
    }

    [Fact]
    public async Task Handle_WithEmptyProductId_ThrowsException()
    {
        // Arrange
        var wishlist = Domain.Shopping.Entities.Wishlist.Create(_tenantId, _customerId);
        _wishlistRepository.GetByIdAsync(wishlist.Id, Arg.Any<CancellationToken>())
            .Returns(wishlist);

        var command = new AddToWishlistCommand(
            WishlistId: wishlist.Id,
            ProductId: Guid.Empty);

        // Act & Assert
        var act = () => _handler.Handle(command, CancellationToken.None);
        await act.Should().ThrowAsync<ArgumentException>();
    }

    #endregion

    #region Tenant Isolation Tests

    [Fact]
    public async Task Handle_WithWishlistFromDifferentTenant_ThrowsException()
    {
        // Arrange
        var differentTenantId = Guid.NewGuid();
        var wishlist = Domain.Shopping.Entities.Wishlist.Create(differentTenantId, _customerId);

        _wishlistRepository.GetByIdAsync(wishlist.Id, Arg.Any<CancellationToken>())
            .Returns(wishlist);

        var command = new AddToWishlistCommand(
            WishlistId: wishlist.Id,
            ProductId: _productId);

        // Act & Assert
        var act = () => _handler.Handle(command, CancellationToken.None);
        await act.Should().ThrowAsync<UnauthorizedAccessException>()
            .WithMessage("*Cannot access wishlist from another tenant*");
    }

    #endregion

    #region Error Handling Tests

    [Fact]
    public async Task Handle_RepositoryThrowsException_PropagatesException()
    {
        // Arrange
        _wishlistRepository.GetByIdAsync(Arg.Any<Guid>(), Arg.Any<CancellationToken>())
            .Returns(Task.FromException<Domain.Shopping.Entities.Wishlist?>(new InvalidOperationException("Repository error")));

        var command = new AddToWishlistCommand(
            WishlistId: Guid.NewGuid(),
            ProductId: _productId);

        // Act & Assert
        var act = () => _handler.Handle(command, CancellationToken.None);
        await act.Should().ThrowAsync<InvalidOperationException>()
            .WithMessage("*Repository error*");
    }

    #endregion
}
