using FluentAssertions;
using KromicStore.Application.Common.Abstractions;
using KromicStore.Application.Features.Catalog.Commands.DeleteProduct;
using KromicStore.Application.Features.Catalog.Commands.RestoreProduct;
using KromicStore.Application.Features.Catalog.Abstractions;
using KromicStore.Application.Tests.Features.Catalog.Common;
using KromicStore.Domain.Catalog.Entities;
using Microsoft.Extensions.Logging;
using NSubstitute;

namespace KromicStore.Application.Tests.Features.Catalog.Commands.ProductLifecycle;

/// <summary>
/// Handler tests for product lifecycle commands (Delete, Restore).
/// Verifies soft delete and restoration business rules.
/// </summary>
public sealed class ProductLifecycleCommandHandlerTests
{
    private readonly IProductRepository _productRepository;
    private readonly IApplicationDbContext _dbContext;
    private readonly ICurrentUserService _currentUserService;
    private readonly ITenantContext _tenantContext;
    private readonly DeleteProductCommandHandler _deleteHandler;
    private readonly RestoreProductCommandHandler _restoreHandler;
    private readonly Guid _tenantId;
    private readonly Guid _categoryId;
    private readonly Guid _productId;

    public ProductLifecycleCommandHandlerTests()
    {
        _tenantId = Guid.NewGuid();
        _categoryId = Guid.NewGuid();
        _productId = Guid.NewGuid();
        _dbContext = Substitute.For<IApplicationDbContext>();
        _tenantContext = CatalogTestFixtures.CreateTenantContext(_tenantId);
        _currentUserService = CatalogTestFixtures.CreateCurrentUserService();
        _productRepository = Substitute.For<IProductRepository>();

        _deleteHandler = new DeleteProductCommandHandler(
            _productRepository,
            _dbContext,
            Substitute.For<ILogger<DeleteProductCommandHandler>>(),
            _tenantContext,
            _currentUserService);

        _restoreHandler = new RestoreProductCommandHandler(
            _productRepository,
            _dbContext,
            Substitute.For<ILogger<RestoreProductCommandHandler>>(),
            _tenantContext,
            _currentUserService);
    }

    #region Delete Tests

    [Fact]
    public async Task DeleteHandle_ActiveProduct_MarksAsDeleted()
    {
        // Arrange
        var product = CatalogTestFixtures.CreateTestProduct(_tenantId, _categoryId);
        product.IsDeleted.Should().BeFalse();

        _productRepository.GetByIdAsync(_productId, Arg.Any<CancellationToken>())
            .Returns(product);

        var command = new DeleteProductCommand(ProductId: _productId);

        // Act
        var result = await _deleteHandler.Handle(command, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result.ProductId.Should().Be(product.Id);
        product.IsDeleted.Should().BeTrue();
    }

    [Fact]
    public async Task DeleteHandle_ProductNotFound_ThrowsException()
    {
        // Arrange
        _productRepository.GetByIdAsync(_productId, Arg.Any<CancellationToken>())
            .Returns((Product?)null);

        var command = new DeleteProductCommand(ProductId: _productId);

        // Act & Assert
        var act = () => _deleteHandler.Handle(command, CancellationToken.None);
        await act.Should().ThrowAsync<InvalidOperationException>();
    }

    [Fact]
    public async Task DeleteHandle_SetsSoftDeleteTimestamp()
    {
        // Arrange
        var product = CatalogTestFixtures.CreateTestProduct(_tenantId, _categoryId);
        _productRepository.GetByIdAsync(_productId, Arg.Any<CancellationToken>())
            .Returns(product);

        var beforeDelete = DateTime.UtcNow;
        var command = new DeleteProductCommand(ProductId: _productId);

        // Act
        await _deleteHandler.Handle(command, CancellationToken.None);

        // Assert
        product.DeletedOnUtc.Should().NotBeNull();
        product.DeletedOnUtc.Should().BeAfter(beforeDelete.AddSeconds(-1));
    }

    [Fact]
    public async Task DeleteHandle_SetsSoftDeleteBy()
    {
        // Arrange
        var product = CatalogTestFixtures.CreateTestProduct(_tenantId, _categoryId);
        _productRepository.GetByIdAsync(_productId, Arg.Any<CancellationToken>())
            .Returns(product);

        var command = new DeleteProductCommand(ProductId: _productId);

        // Act
        await _deleteHandler.Handle(command, CancellationToken.None);

        // Assert
        product.DeletedBy.Should().NotBeNullOrEmpty();
    }

    [Fact]
    public async Task DeleteHandle_PersistsChanges()
    {
        // Arrange
        var product = CatalogTestFixtures.CreateTestProduct(_tenantId, _categoryId);
        _productRepository.GetByIdAsync(_productId, Arg.Any<CancellationToken>())
            .Returns(product);

        var command = new DeleteProductCommand(ProductId: _productId);

        // Act
        await _deleteHandler.Handle(command, CancellationToken.None);

        // Assert
        await _dbContext.Received(1).SaveChangesAsync(Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task DeleteHandle_AlreadyDeleted_StillSucceeds()
    {
        // Arrange
        var product = CatalogTestFixtures.CreateTestProduct(_tenantId, _categoryId);
        product.SoftDelete(DateTime.UtcNow, "previous-user");
        product.IsDeleted.Should().BeTrue();

        _productRepository.GetByIdAsync(_productId, Arg.Any<CancellationToken>())
            .Returns(product);

        var command = new DeleteProductCommand(ProductId: _productId);

        // Act
        var result = await _deleteHandler.Handle(command, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        product.IsDeleted.Should().BeTrue();
    }

    #endregion

    #region Restore Tests

    [Fact]
    public async Task RestoreHandle_DeletedProduct_RemovesSoftDeleteFlag()
    {
        // Arrange
        var product = CatalogTestFixtures.CreateTestProduct(_tenantId, _categoryId);
        product.SoftDelete(DateTime.UtcNow, "test-user");
        product.IsDeleted.Should().BeTrue();

        _productRepository.GetByIdAsync(_productId, Arg.Any<CancellationToken>())
            .Returns(product);

        var command = new RestoreProductCommand(ProductId: _productId);

        // Act
        var result = await _restoreHandler.Handle(command, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result.ProductId.Should().Be(product.Id);
        product.IsDeleted.Should().BeFalse();
    }

    [Fact]
    public async Task RestoreHandle_ProductNotFound_ThrowsException()
    {
        // Arrange
        _productRepository.GetByIdAsync(_productId, Arg.Any<CancellationToken>())
            .Returns((Product?)null);

        var command = new RestoreProductCommand(ProductId: _productId);

        // Act & Assert
        var act = () => _restoreHandler.Handle(command, CancellationToken.None);
        await act.Should().ThrowAsync<InvalidOperationException>();
    }

    [Fact]
    public async Task RestoreHandle_ClearsDeletedOnUtc()
    {
        // Arrange
        var product = CatalogTestFixtures.CreateTestProduct(_tenantId, _categoryId);
        product.SoftDelete(DateTime.UtcNow, "test-user");
        product.DeletedOnUtc.Should().NotBeNull();

        _productRepository.GetByIdAsync(_productId, Arg.Any<CancellationToken>())
            .Returns(product);

        var command = new RestoreProductCommand(ProductId: _productId);

        // Act
        await _restoreHandler.Handle(command, CancellationToken.None);

        // Assert
        product.DeletedOnUtc.Should().BeNull();
    }

    [Fact]
    public async Task RestoreHandle_ClearsDeletedBy()
    {
        // Arrange
        var product = CatalogTestFixtures.CreateTestProduct(_tenantId, _categoryId);
        product.SoftDelete(DateTime.UtcNow, "test-user");
        product.DeletedBy.Should().NotBeNull();

        _productRepository.GetByIdAsync(_productId, Arg.Any<CancellationToken>())
            .Returns(product);

        var command = new RestoreProductCommand(ProductId: _productId);

        // Act
        await _restoreHandler.Handle(command, CancellationToken.None);

        // Assert
        product.DeletedBy.Should().BeNull();
    }

    [Fact]
    public async Task RestoreHandle_PersistsChanges()
    {
        // Arrange
        var product = CatalogTestFixtures.CreateTestProduct(_tenantId, _categoryId);
        product.SoftDelete(DateTime.UtcNow, "test-user");
        _productRepository.GetByIdAsync(_productId, Arg.Any<CancellationToken>())
            .Returns(product);

        var command = new RestoreProductCommand(ProductId: _productId);

        // Act
        await _restoreHandler.Handle(command, CancellationToken.None);

        // Assert
        await _dbContext.Received(1).SaveChangesAsync(Arg.Any<CancellationToken>());
    }

    #endregion

    #region Combined Lifecycle Tests

    [Fact]
    public async Task DeleteAndRestore_FullCycle()
    {
        // Arrange
        var product = CatalogTestFixtures.CreateTestProduct(_tenantId, _categoryId);
        _productRepository.GetByIdAsync(_productId, Arg.Any<CancellationToken>())
            .Returns(product);

        // Act - Delete
        await _deleteHandler.Handle(new DeleteProductCommand(ProductId: _productId), CancellationToken.None);
        product.IsDeleted.Should().BeTrue();

        // Act - Restore
        await _restoreHandler.Handle(new RestoreProductCommand(ProductId: _productId), CancellationToken.None);

        // Assert
        product.IsDeleted.Should().BeFalse();
        product.DeletedOnUtc.Should().BeNull();
        product.DeletedBy.Should().BeNull();
    }

    [Fact]
    public async Task MultipleDeleteRestore_CyclesCorrectly()
    {
        // Arrange
        var product = CatalogTestFixtures.CreateTestProduct(_tenantId, _categoryId);
        _productRepository.GetByIdAsync(_productId, Arg.Any<CancellationToken>())
            .Returns(product);

        // Cycle 1: Delete and Restore
        await _deleteHandler.Handle(new DeleteProductCommand(ProductId: _productId), CancellationToken.None);
        product.IsDeleted.Should().BeTrue();

        await _restoreHandler.Handle(new RestoreProductCommand(ProductId: _productId), CancellationToken.None);
        product.IsDeleted.Should().BeFalse();

        // Cycle 2: Delete again and Restore again
        await _deleteHandler.Handle(new DeleteProductCommand(ProductId: _productId), CancellationToken.None);
        product.IsDeleted.Should().BeTrue();

        await _restoreHandler.Handle(new RestoreProductCommand(ProductId: _productId), CancellationToken.None);
        product.IsDeleted.Should().BeFalse();
    }

    #endregion
}
