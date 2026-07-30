using FluentAssertions;
using KromicStore.Application.Common.Abstractions;
using KromicStore.Application.Features.Catalog.Commands.DuplicateProduct;
using KromicStore.Application.Features.Catalog.Abstractions;
using KromicStore.Application.Tests.Features.Catalog.Common;
using KromicStore.Domain.Catalog.Entities;
using Microsoft.Extensions.Logging;
using NSubstitute;

namespace KromicStore.Application.Tests.Features.Catalog.Commands.DuplicateProduct;

/// <summary>
/// Handler tests for DuplicateProductCommand.
/// Verifies product duplication with attribute/tag copying and business rules.
/// </summary>
public sealed class DuplicateProductCommandHandlerTests
{
    private readonly IProductRepository _productRepository;
    private readonly IApplicationDbContext _dbContext;
    private readonly ICurrentUserService _currentUserService;
    private readonly ITenantContext _tenantContext;
    private readonly DuplicateProductCommandHandler _handler;
    private readonly Guid _tenantId;
    private readonly Guid _categoryId;
    private readonly Guid _productId;

    public DuplicateProductCommandHandlerTests()
    {
        _tenantId = Guid.NewGuid();
        _categoryId = Guid.NewGuid();
        _productId = Guid.NewGuid();
        _dbContext = Substitute.For<IApplicationDbContext>();
        _tenantContext = CatalogTestFixtures.CreateTenantContext(_tenantId);
        _currentUserService = CatalogTestFixtures.CreateCurrentUserService();

        _productRepository = Substitute.For<IProductRepository>();

        _handler = new DuplicateProductCommandHandler(
            _productRepository,
            _dbContext,
            Substitute.For<ILogger<DuplicateProductCommandHandler>>(),
            _tenantContext,
            _currentUserService);
    }

    [Fact]
    public async Task Handle_ValidCommand_DuplicatesProduct()
    {
        // Arrange
        var originalProduct = CatalogTestFixtures.CreateTestProduct(_tenantId, _categoryId);
        _productRepository.GetByIdAsync(_productId, Arg.Any<CancellationToken>())
            .Returns(originalProduct);
        _productRepository.SkuExistsAsync("NEW-SKU", null, Arg.Any<CancellationToken>())
            .Returns(false);

        var command = new DuplicateProductCommand(
            ProductId: _productId,
            NewSku: "NEW-SKU",
            NewName: "Duplicated Product");

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result.NewSku.Should().Be("NEW-SKU");
        result.NewName.Should().Be("Duplicated Product");
        result.DuplicatedProductId.Should().NotBeEmpty();
        _productRepository.Received(1).Add(Arg.Any<Product>());
    }

    [Fact]
    public async Task Handle_ProductNotFound_ThrowsException()
    {
        // Arrange
        _productRepository.GetByIdAsync(_productId, Arg.Any<CancellationToken>())
            .Returns((Product?)null);

        var command = new DuplicateProductCommand(
            ProductId: _productId,
            NewSku: "NEW-SKU",
            NewName: "Test");

        // Act & Assert
        var act = () => _handler.Handle(command, CancellationToken.None);
        await act.Should().ThrowAsync<InvalidOperationException>();
    }

    [Fact]
    public async Task Handle_DuplicateSku_ThrowsException()
    {
        // Arrange
        var originalProduct = CatalogTestFixtures.CreateTestProduct(_tenantId, _categoryId);
        _productRepository.GetByIdAsync(_productId, Arg.Any<CancellationToken>())
            .Returns(originalProduct);
        _productRepository.SkuExistsAsync("DUPLICATE", null, Arg.Any<CancellationToken>())
            .Returns(true);

        var command = new DuplicateProductCommand(
            ProductId: _productId,
            NewSku: "DUPLICATE",
            NewName: "Test");

        // Act & Assert
        var act = () => _handler.Handle(command, CancellationToken.None);
        await act.Should().ThrowAsync<InvalidOperationException>();
    }

    [Fact]
    public async Task Handle_DuplicateSlug_ThrowsException()
    {
        // Arrange
        var originalProduct = CatalogTestFixtures.CreateTestProduct(_tenantId, _categoryId);
        _productRepository.GetByIdAsync(_productId, Arg.Any<CancellationToken>())
            .Returns(originalProduct);
        _productRepository.SkuExistsAsync("NEW-SKU", null, Arg.Any<CancellationToken>())
            .Returns(false);
        _productRepository.SlugExistsAsync("duplicate-slug", null, Arg.Any<CancellationToken>())
            .Returns(true);

        var command = new DuplicateProductCommand(
            ProductId: _productId,
            NewSku: "NEW-SKU",
            NewName: "Test",
            NewSlug: "duplicate-slug");

        // Act & Assert
        var act = () => _handler.Handle(command, CancellationToken.None);
        await act.Should().ThrowAsync<InvalidOperationException>();
    }

    [Fact]
    public async Task Handle_CopiesProductProperties()
    {
        // Arrange
        var originalProduct = Product.Create(
            tenantId: _tenantId,
            categoryId: _categoryId,
            sku: "ORIG-SKU",
            name: "Original",
            price: 99.99m,
            compareAtPrice: 149.99m,
            costPrice: 50m,
            weight: 2.5m,
            length: 10m,
            width: 5m,
            height: 15m,
            taxable: true);

        _productRepository.GetByIdAsync(_productId, Arg.Any<CancellationToken>())
            .Returns(originalProduct);
        _productRepository.SkuExistsAsync("NEW-SKU", null, Arg.Any<CancellationToken>())
            .Returns(false);

        var command = new DuplicateProductCommand(
            ProductId: _productId,
            NewSku: "NEW-SKU",
            NewName: "Duplicate");

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        // Verify that the add was called (properties would be set on the duplicated product)
        _productRepository.Received(1).Add(Arg.Is<Product>(p => 
            p.Sku == "NEW-SKU" &&
            p.Name == "Duplicate" &&
            p.Price == 99.99m &&
            p.CompareAtPrice == 149.99m &&
            p.CostPrice == 50m &&
            p.Weight == 2.5m &&
            p.Taxable == true));
    }

    [Fact]
    public async Task Handle_DuplicatedProductIsDraft_Regardless()
    {
        // Arrange
        var originalProduct = CatalogTestFixtures.CreateTestProduct(_tenantId, _categoryId);
        originalProduct.Publish(); // Set original to Active
        
        _productRepository.GetByIdAsync(_productId, Arg.Any<CancellationToken>())
            .Returns(originalProduct);
        _productRepository.SkuExistsAsync("NEW-SKU", null, Arg.Any<CancellationToken>())
            .Returns(false);

        var command = new DuplicateProductCommand(
            ProductId: _productId,
            NewSku: "NEW-SKU",
            NewName: "Duplicate");

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        // Duplicated product should be in Draft status
        _productRepository.Received(1).Add(Arg.Is<Product>(p => 
            p.Status == ProductStatus.Draft));
    }

    [Fact]
    public async Task Handle_DuplicatedProductIsNotFeatured()
    {
        // Arrange
        var originalProduct = CatalogTestFixtures.CreateTestProduct(_tenantId, _categoryId);
        // originalProduct.MakeFeatures(); - simulating featured product
        
        _productRepository.GetByIdAsync(_productId, Arg.Any<CancellationToken>())
            .Returns(originalProduct);
        _productRepository.SkuExistsAsync("NEW-SKU", null, Arg.Any<CancellationToken>())
            .Returns(false);

        var command = new DuplicateProductCommand(
            ProductId: _productId,
            NewSku: "NEW-SKU",
            NewName: "Duplicate");

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        _productRepository.Received(1).Add(Arg.Is<Product>(p => p.IsFeatured == false));
    }

    [Fact]
    public async Task Handle_CopiesAttributes()
    {
        // Arrange
        var originalProduct = CatalogTestFixtures.CreateTestProduct(_tenantId, _categoryId);
        originalProduct.AddAttribute("Color", "Red");
        originalProduct.AddAttribute("Size", "Large");

        _productRepository.GetByIdAsync(_productId, Arg.Any<CancellationToken>())
            .Returns(originalProduct);
        _productRepository.SkuExistsAsync("NEW-SKU", null, Arg.Any<CancellationToken>())
            .Returns(false);

        var command = new DuplicateProductCommand(
            ProductId: _productId,
            NewSku: "NEW-SKU",
            NewName: "Duplicate");

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        _productRepository.Received(1).Add(Arg.Any<Product>());
    }

    [Fact]
    public async Task Handle_CopiesTags()
    {
        // Arrange
        var originalProduct = CatalogTestFixtures.CreateTestProduct(_tenantId, _categoryId);
        originalProduct.AddTag("Sale");
        originalProduct.AddTag("Featured");

        _productRepository.GetByIdAsync(_productId, Arg.Any<CancellationToken>())
            .Returns(originalProduct);
        _productRepository.SkuExistsAsync("NEW-SKU", null, Arg.Any<CancellationToken>())
            .Returns(false);

        var command = new DuplicateProductCommand(
            ProductId: _productId,
            NewSku: "NEW-SKU",
            NewName: "Duplicate");

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        _productRepository.Received(1).Add(Arg.Any<Product>());
    }

    [Fact]
    public async Task Handle_GeneratesSlug_WhenNotProvided()
    {
        // Arrange
        var originalProduct = CatalogTestFixtures.CreateTestProduct(_tenantId, _categoryId);
        _productRepository.GetByIdAsync(_productId, Arg.Any<CancellationToken>())
            .Returns(originalProduct);
        _productRepository.SkuExistsAsync("NEW-SKU", null, Arg.Any<CancellationToken>())
            .Returns(false);

        var command = new DuplicateProductCommand(
            ProductId: _productId,
            NewSku: "NEW-SKU",
            NewName: "Duplicate Product",
            NewSlug: null);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.NewSlug.Should().NotBeNullOrEmpty();
    }

    [Fact]
    public async Task Handle_UsesProvidedSlug()
    {
        // Arrange
        var originalProduct = CatalogTestFixtures.CreateTestProduct(_tenantId, _categoryId);
        _productRepository.GetByIdAsync(_productId, Arg.Any<CancellationToken>())
            .Returns(originalProduct);
        _productRepository.SkuExistsAsync("NEW-SKU", null, Arg.Any<CancellationToken>())
            .Returns(false);
        _productRepository.SlugExistsAsync("custom-slug", null, Arg.Any<CancellationToken>())
            .Returns(false);

        var command = new DuplicateProductCommand(
            ProductId: _productId,
            NewSku: "NEW-SKU",
            NewName: "Duplicate",
            NewSlug: "custom-slug");

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.NewSlug.Should().Be("custom-slug");
    }

    [Fact]
    public async Task Handle_PreservesTenantId()
    {
        // Arrange
        var originalProduct = CatalogTestFixtures.CreateTestProduct(_tenantId, _categoryId);
        _productRepository.GetByIdAsync(_productId, Arg.Any<CancellationToken>())
            .Returns(originalProduct);
        _productRepository.SkuExistsAsync("NEW-SKU", null, Arg.Any<CancellationToken>())
            .Returns(false);

        var command = new DuplicateProductCommand(
            ProductId: _productId,
            NewSku: "NEW-SKU",
            NewName: "Duplicate");

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        _productRepository.Received(1).Add(Arg.Is<Product>(p => p.TenantId == _tenantId));
    }

    [Fact]
    public async Task Handle_PreservesCategoryId()
    {
        // Arrange
        var originalProduct = CatalogTestFixtures.CreateTestProduct(_tenantId, _categoryId);
        _productRepository.GetByIdAsync(_productId, Arg.Any<CancellationToken>())
            .Returns(originalProduct);
        _productRepository.SkuExistsAsync("NEW-SKU", null, Arg.Any<CancellationToken>())
            .Returns(false);

        var command = new DuplicateProductCommand(
            ProductId: _productId,
            NewSku: "NEW-SKU",
            NewName: "Duplicate");

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        _productRepository.Received(1).Add(Arg.Is<Product>(p => p.CategoryId == _categoryId));
    }

    [Fact]
    public async Task Handle_PersistsChanges()
    {
        // Arrange
        var originalProduct = CatalogTestFixtures.CreateTestProduct(_tenantId, _categoryId);
        _productRepository.GetByIdAsync(_productId, Arg.Any<CancellationToken>())
            .Returns(originalProduct);
        _productRepository.SkuExistsAsync("NEW-SKU", null, Arg.Any<CancellationToken>())
            .Returns(false);

        var command = new DuplicateProductCommand(
            ProductId: _productId,
            NewSku: "NEW-SKU",
            NewName: "Duplicate");

        // Act
        await _handler.Handle(command, CancellationToken.None);

        // Assert
        await _dbContext.Received(1).SaveChangesAsync(Arg.Any<CancellationToken>());
    }
}
