using FluentAssertions;
using KromicStore.Application.Common.Abstractions;
using KromicStore.Application.Features.Catalog.Commands.UpdateProduct;
using KromicStore.Application.Features.Catalog.Abstractions;
using KromicStore.Application.Tests.Features.Catalog.Common;
using KromicStore.Domain.Catalog.Entities;
using Microsoft.Extensions.Logging;
using NSubstitute;

namespace KromicStore.Application.Tests.Features.Catalog.Commands.UpdateProduct;

/// <summary>
/// Handler tests for UpdateProductCommand.
/// Verifies product updates with validation, persistence, and business rules.
/// </summary>
public sealed class UpdateProductCommandHandlerTests
{
    private readonly IProductRepository _productRepository;
    private readonly ICategoryRepository _categoryRepository;
    private readonly IApplicationDbContext _dbContext;
    private readonly ICurrentUserService _currentUserService;
    private readonly ITenantContext _tenantContext;
    private readonly UpdateProductCommandHandler _handler;
    private readonly Guid _tenantId;
    private readonly Guid _categoryId;
    private readonly Guid _productId;

    public UpdateProductCommandHandlerTests()
    {
        _tenantId = Guid.NewGuid();
        _categoryId = Guid.NewGuid();
        _productId = Guid.NewGuid();
        _dbContext = Substitute.For<IApplicationDbContext>();
        _tenantContext = CatalogTestFixtures.CreateTenantContext(_tenantId);
        _currentUserService = CatalogTestFixtures.CreateCurrentUserService();

        _productRepository = Substitute.For<IProductRepository>();
        _categoryRepository = Substitute.For<ICategoryRepository>();

        _handler = new UpdateProductCommandHandler(
            _productRepository,
            _categoryRepository,
            _dbContext,
            Substitute.For<ILogger<UpdateProductCommandHandler>>(),
            _tenantContext,
            _currentUserService);
    }

    [Fact]
    public async Task Handle_ValidCommand_UpdatesProduct()
    {
        // Arrange
        var product = CatalogTestFixtures.CreateTestProduct(_tenantId, _categoryId);
        _productRepository.GetByIdAsync(_productId, Arg.Any<CancellationToken>())
            .Returns(product);
        _productRepository.SkuExistsAsync(Arg.Any<string>(), _productId, Arg.Any<CancellationToken>())
            .Returns(false);

        var command = new UpdateProductCommand(
            ProductId: _productId,
            Name: "Updated Product",
            Price: 99.99m);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result.Name.Should().Be("Updated Product");
        await _dbContext.Received(1).SaveChangesAsync(Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Handle_ProductNotFound_ThrowsException()
    {
        // Arrange
        _productRepository.GetByIdAsync(_productId, Arg.Any<CancellationToken>())
            .Returns((Product?)null);

        var command = new UpdateProductCommand(
            ProductId: _productId,
            Name: "Nonexistent");

        // Act & Assert
        var act = () => _handler.Handle(command, CancellationToken.None);
        await act.Should().ThrowAsync<InvalidOperationException>();
    }

    [Fact]
    public async Task Handle_CategoryNotFound_ThrowsException()
    {
        // Arrange
        var product = CatalogTestFixtures.CreateTestProduct(_tenantId, _categoryId);
        _productRepository.GetByIdAsync(_productId, Arg.Any<CancellationToken>())
            .Returns(product);
        _categoryRepository.GetByIdAsync(Arg.Any<Guid>(), Arg.Any<CancellationToken>())
            .Returns((Category?)null);

        var newCategoryId = Guid.NewGuid();
        var command = new UpdateProductCommand(
            ProductId: _productId,
            CategoryId: newCategoryId);

        // Act & Assert
        var act = () => _handler.Handle(command, CancellationToken.None);
        await act.Should().ThrowAsync<InvalidOperationException>();
    }

    [Fact]
    public async Task Handle_DuplicateSku_ThrowsException()
    {
        // Arrange
        var product = CatalogTestFixtures.CreateTestProduct(_tenantId, _categoryId);
        _productRepository.GetByIdAsync(_productId, Arg.Any<CancellationToken>())
            .Returns(product);
        _productRepository.SkuExistsAsync("DUPLICATE", _productId, Arg.Any<CancellationToken>())
            .Returns(true);

        var command = new UpdateProductCommand(
            ProductId: _productId,
            Sku: "DUPLICATE");

        // Act & Assert
        var act = () => _handler.Handle(command, CancellationToken.None);
        await act.Should().ThrowAsync<InvalidOperationException>();
    }

    [Fact]
    public async Task Handle_DuplicateSlug_ThrowsException()
    {
        // Arrange
        var product = CatalogTestFixtures.CreateTestProduct(_tenantId, _categoryId);
        _productRepository.GetByIdAsync(_productId, Arg.Any<CancellationToken>())
            .Returns(product);
        _productRepository.SkuExistsAsync(Arg.Any<string>(), _productId, Arg.Any<CancellationToken>())
            .Returns(false);
        _productRepository.SlugExistsAsync("duplicate-slug", _productId, Arg.Any<CancellationToken>())
            .Returns(true);

        var command = new UpdateProductCommand(
            ProductId: _productId,
            CustomSlug: "duplicate-slug");

        // Act & Assert
        var act = () => _handler.Handle(command, CancellationToken.None);
        await act.Should().ThrowAsync<InvalidOperationException>();
    }

    [Fact]
    public async Task Handle_UpdateName_ChangesName()
    {
        // Arrange
        var product = CatalogTestFixtures.CreateTestProduct(_tenantId, _categoryId);
        _productRepository.GetByIdAsync(_productId, Arg.Any<CancellationToken>())
            .Returns(product);

        var command = new UpdateProductCommand(
            ProductId: _productId,
            Name: "New Name");

        // Act
        await _handler.Handle(command, CancellationToken.None);

        // Assert
        product.Name.Should().Be("New Name");
    }

    [Fact]
    public async Task Handle_UpdatePrice_ChangesPrice()
    {
        // Arrange
        var product = CatalogTestFixtures.CreateTestProduct(_tenantId, _categoryId);
        _productRepository.GetByIdAsync(_productId, Arg.Any<CancellationToken>())
            .Returns(product);

        var command = new UpdateProductCommand(
            ProductId: _productId,
            Price: 199.99m);

        // Act
        await _handler.Handle(command, CancellationToken.None);

        // Assert
        product.Price.Should().Be(199.99m);
    }

    [Fact]
    public async Task Handle_UpdateDescription_ChangesDescription()
    {
        // Arrange
        var product = CatalogTestFixtures.CreateTestProduct(_tenantId, _categoryId);
        _productRepository.GetByIdAsync(_productId, Arg.Any<CancellationToken>())
            .Returns(product);

        var newDescription = "Updated description text";
        var command = new UpdateProductCommand(
            ProductId: _productId,
            Description: newDescription);

        // Act
        await _handler.Handle(command, CancellationToken.None);

        // Assert
        product.Description.Should().Be(newDescription);
    }

    [Fact]
    public async Task Handle_UpdateStatus_ChangesStatus()
    {
        // Arrange
        var product = CatalogTestFixtures.CreateTestProduct(_tenantId, _categoryId);
        _productRepository.GetByIdAsync(_productId, Arg.Any<CancellationToken>())
            .Returns(product);

        var command = new UpdateProductCommand(
            ProductId: _productId,
            Status: "Active");

        // Act
        await _handler.Handle(command, CancellationToken.None);

        // Assert
        product.Status.Should().Be(ProductStatus.Active);
    }

    [Fact]
    public async Task Handle_UpdateMultipleFields_UpdatesAllFields()
    {
        // Arrange
        var product = CatalogTestFixtures.CreateTestProduct(_tenantId, _categoryId);
        _productRepository.GetByIdAsync(_productId, Arg.Any<CancellationToken>())
            .Returns(product);
        _productRepository.SkuExistsAsync("NEW-SKU", _productId, Arg.Any<CancellationToken>())
            .Returns(false);

        var command = new UpdateProductCommand(
            ProductId: _productId,
            Name: "Multi Update",
            Sku: "NEW-SKU",
            Price: 299.99m,
            IsFeatured: true);

        // Act
        await _handler.Handle(command, CancellationToken.None);

        // Assert
        product.Name.Should().Be("Multi Update");
        product.Sku.Should().Be("NEW-SKU");
        product.Price.Should().Be(299.99m);
        product.IsFeatured.Should().BeTrue();
    }

    [Fact]
    public async Task Handle_UpdateWithNullValues_IgnoresNullFields()
    {
        // Arrange
        var originalName = "Original";
        var product = Product.Create(
            tenantId: _tenantId,
            categoryId: _categoryId,
            sku: "SKU-001",
            name: originalName,
            price: 50m);

        _productRepository.GetByIdAsync(_productId, Arg.Any<CancellationToken>())
            .Returns(product);

        var command = new UpdateProductCommand(
            ProductId: _productId,
            Name: null,
            Price: null);

        // Act
        await _handler.Handle(command, CancellationToken.None);

        // Assert
        product.Name.Should().Be(originalName);
        product.Price.Should().Be(50m);
    }

    [Fact]
    public async Task Handle_UpdateMarkModified_SetsModifiedTimestamp()
    {
        // Arrange
        var product = CatalogTestFixtures.CreateTestProduct(_tenantId, _categoryId);
        var originalModified = product.ModifiedAtUtc;
        
        _productRepository.GetByIdAsync(_productId, Arg.Any<CancellationToken>())
            .Returns(product);

        var command = new UpdateProductCommand(
            ProductId: _productId,
            Name: "Modified");

        // Act
        await _handler.Handle(command, CancellationToken.None);

        // Assert
        product.ModifiedAtUtc.Should().BeAfter(originalModified);
    }

    [Fact]
    public async Task Handle_UpdateDimensions_UpdatesDimensionFields()
    {
        // Arrange
        var product = CatalogTestFixtures.CreateTestProduct(_tenantId, _categoryId);
        _productRepository.GetByIdAsync(_productId, Arg.Any<CancellationToken>())
            .Returns(product);

        var command = new UpdateProductCommand(
            ProductId: _productId,
            Weight: 2.5m,
            Length: 10m,
            Width: 5m,
            Height: 15m);

        // Act
        await _handler.Handle(command, CancellationToken.None);

        // Assert
        product.Weight.Should().Be(2.5m);
        product.Length.Should().Be(10m);
        product.Width.Should().Be(5m);
        product.Height.Should().Be(15m);
    }

    [Fact]
    public async Task Handle_UpdateCompareAtPrice_UpdatesPriceFields()
    {
        // Arrange
        var product = CatalogTestFixtures.CreateTestProduct(_tenantId, _categoryId);
        _productRepository.GetByIdAsync(_productId, Arg.Any<CancellationToken>())
            .Returns(product);

        var command = new UpdateProductCommand(
            ProductId: _productId,
            Price: 99.99m,
            CompareAtPrice: 149.99m,
            CostPrice: 50m);

        // Act
        await _handler.Handle(command, CancellationToken.None);

        // Assert
        product.Price.Should().Be(99.99m);
        product.CompareAtPrice.Should().Be(149.99m);
        product.CostPrice.Should().Be(50m);
    }

    [Fact]
    public async Task Handle_UpdateIsFeatured_TogglesFeaturedStatus()
    {
        // Arrange
        var product = CatalogTestFixtures.CreateTestProduct(_tenantId, _categoryId);
        product.Should().NotBeNull();
        
        _productRepository.GetByIdAsync(_productId, Arg.Any<CancellationToken>())
            .Returns(product);

        var command = new UpdateProductCommand(
            ProductId: _productId,
            IsFeatured: true);

        // Act
        await _handler.Handle(command, CancellationToken.None);

        // Assert
        product.IsFeatured.Should().BeTrue();
    }

    [Fact]
    public async Task Handle_UpdateTaxable_TogglesTaxableStatus()
    {
        // Arrange
        var product = CatalogTestFixtures.CreateTestProduct(_tenantId, _categoryId);
        _productRepository.GetByIdAsync(_productId, Arg.Any<CancellationToken>())
            .Returns(product);

        var command = new UpdateProductCommand(
            ProductId: _productId,
            Taxable: false);

        // Act
        await _handler.Handle(command, CancellationToken.None);

        // Assert
        product.Taxable.Should().BeFalse();
    }

    [Fact]
    public async Task Handle_UpdateToNewCategory_ChangesCategoryId()
    {
        // Arrange
        var product = CatalogTestFixtures.CreateTestProduct(_tenantId, _categoryId);
        var newCategoryId = Guid.NewGuid();
        var newCategory = CatalogTestFixtures.CreateTestCategory(_tenantId);
        
        _productRepository.GetByIdAsync(_productId, Arg.Any<CancellationToken>())
            .Returns(product);
        _categoryRepository.GetByIdAsync(newCategoryId, Arg.Any<CancellationToken>())
            .Returns(newCategory);

        var command = new UpdateProductCommand(
            ProductId: _productId,
            CategoryId: newCategoryId);

        // Act
        await _handler.Handle(command, CancellationToken.None);

        // Assert
        product.CategoryId.Should().Be(newCategoryId);
    }

    [Fact]
    public async Task Handle_PersistsChanges()
    {
        // Arrange
        var product = CatalogTestFixtures.CreateTestProduct(_tenantId, _categoryId);
        _productRepository.GetByIdAsync(_productId, Arg.Any<CancellationToken>())
            .Returns(product);

        var command = new UpdateProductCommand(
            ProductId: _productId,
            Name: "Persist Test");

        // Act
        await _handler.Handle(command, CancellationToken.None);

        // Assert
        await _dbContext.Received(1).SaveChangesAsync(Arg.Any<CancellationToken>());
    }
}
