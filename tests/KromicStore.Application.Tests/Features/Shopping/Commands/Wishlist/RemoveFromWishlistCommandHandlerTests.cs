using FluentAssertions;
using KromicStore.Application.Common.Abstractions;
using KromicStore.Application.Features.Shopping.Abstractions;
using KromicStore.Application.Features.Shopping.Commands.RemoveFromWishlist;
using KromicStore.Application.Tests.Features.Shopping.Common;
using KromicStore.Infrastructure.Persistence;
using Microsoft.Extensions.Logging;
using NSubstitute;
using Xunit;

namespace KromicStore.Application.Tests.Features.Shopping.Commands.Wishlist;

/// <summary>
/// Tests for RemoveFromWishlistCommandHandler.
/// Verifies product removal from wishlist, non-existent item handling, authorization, and error handling.
/// </summary>
public sealed class RemoveFromWishlistCommandHandlerTests
{
    private readonly IWishlistRepository _wishlistRepository;
    private readonly KromicStoreDbContext _dbContext;
    private readonly ITenantContext _tenantContext;
    private readonly RemoveFromWishlistCommandHandler _handler;
    private readonly Guid _tenantId;
    private readonly Guid _customerId;
    private readonly Guid _productId;

    public RemoveFromWishlistCommandHandlerTests()
    {
        _tenantId = Guid.NewGuid();
        _customerId = Guid.NewGuid();
        _productId = Guid.NewGuid();
        _dbContext = ShoppingTestFixtures.CreateDbContext(_tenantId);
        _tenantContext = ShoppingTestFixtures.CreateTenantContext(_tenantId);

        _wishlistRepository = Substitute.For<IWishlistRepository>();

        _handler = new RemoveFromWishlistCommandHandler(
            _wishlistRepository,
            _dbContext,
            Substitute.For<ILogger<RemoveFromWishlistCommandHandler>>(),
            _tenantContext);
    }

    #region Success Tests

    [Fact]
    public async Task Handle_WithExistingProduct_RemovesFromWishlist()
    {
        // Arrange
        var wishlist = Domain.Shopping.Entities.Wishlist.Create(_tenantId, _customerId);
        wishlist.AddItem(_productId);

        _wishlistRepository.GetByIdAsync(wishlist.Id, Arg.Any<CancellationToken>())
            .Returns(wishlist);

        var command = new RemoveFromWishlistCommand(
            WishlistId: wishlist.Id,
            ProductId: _productId);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result.WishlistId.Should().Be(wishlist.Id);
        result.ProductId.Should().Be(_productId);
        result.WasRemoved.Should().BeTrue();
        result.ItemsCount.Should().Be(0);
    }

    [Fact]
    public async Task Handle_WithMultipleProducts_RemovesSpecificProduct()
    {
        // Arrange
        var wishlist = Domain.Shopping.Entities.Wishlist.Create(_tenantId, _customerId);
        var product1Id = Guid.NewGuid();
        var product2Id = Guid.NewGuid();

        wishlist.AddItem(product1Id);
        wishlist.AddItem(product2Id);

        _wishlistRepository.GetByIdAsync(wishlist.Id, Arg.Any<CancellationToken>())
            .Returns(wishlist);

        var command = new RemoveFromWishlistCommand(
            WishlistId: wishlist.Id,
            ProductId: product2Id);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.WasRemoved.Should().BeTrue();
        result.ItemsCount.Should().Be(1);
        wishlist.ContainsProduct(product1Id).Should().BeTrue();
        wishlist.ContainsProduct(product2Id).Should().BeFalse();
    }

    #endregion

    #region Non-Existent Item Tests

    [Fact]
    public async Task Handle_WithNonExistentProduct_DoesNotThrow()
    {
        // Arrange
        var wishlist = Domain.Shopping.Entities.Wishlist.Create(_tenantId, _customerId);
        var nonExistentProductId = Guid.NewGuid();

        _wishlistRepository.GetByIdAsync(wishlist.Id, Arg.Any<CancellationToken>())
            .Returns(wishlist);

        var command = new RemoveFromWishlistCommand(
            WishlistId: wishlist.Id,
            ProductId: nonExistentProductId);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.WasRemoved.Should().BeFalse();
        result.ItemsCount.Should().Be(0);
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

        var command = new RemoveFromWishlistCommand(
            WishlistId: wishlistId,
            ProductId: _productId);

        // Act & Assert
        var act = () => _handler.Handle(command, CancellationToken.None);
        await act.Should().ThrowAsync<InvalidOperationException>()
            .WithMessage("*not found*");
    }

    #endregion

    #region Tenant Isolation Tests

    [Fact]
    public async Task Handle_WithWishlistFromDifferentTenant_ThrowsException()
    {
        // Arrange
        var differentTenantId = Guid.NewGuid();
        var wishlist = Domain.Shopping.Entities.Wishlist.Create(differentTenantId, _customerId);
        wishlist.AddItem(_productId);

        _wishlistRepository.GetByIdAsync(wishlist.Id, Arg.Any<CancellationToken>())
            .Returns(wishlist);

        var command = new RemoveFromWishlistCommand(
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

        var command = new RemoveFromWishlistCommand(
            WishlistId: Guid.NewGuid(),
            ProductId: _productId);

        // Act & Assert
        var act = () => _handler.Handle(command, CancellationToken.None);
        await act.Should().ThrowAsync<InvalidOperationException>()
            .WithMessage("*Repository error*");
    }

    #endregion
}
