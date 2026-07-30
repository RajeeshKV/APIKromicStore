using FluentAssertions;
using KromicStore.Application.Common.Abstractions;
using KromicStore.Application.Features.Catalog.Commands.CreateProduct;
using KromicStore.Application.Features.Catalog.Abstractions;
using KromicStore.Application.Tests.Features.Catalog.Common;
using KromicStore.Domain.Catalog.Entities;
using KromicStore.Infrastructure.Persistence;
using Microsoft.Extensions.Logging;
using NSubstitute;

namespace KromicStore.Application.Tests.Features.Catalog.Commands.CreateProduct;

/// <summary>
/// Handler tests for CreateProductCommand.
/// Verifies product creation with validation, persistence, and event handling.
/// </summary>
public sealed class CreateProductCommandHandlerTests
{
    private readonly IProductRepository _productRepository;
    private readonly ICategoryRepository _categoryRepository;
    private readonly KromicStoreDbContext _dbContext;
    private readonly ICurrentUserService _currentUserService;
    private readonly ITenantContext _tenantContext;
    private readonly CreateProductCommandHandler _handler;
    private readonly Guid _tenantId;
    private readonly Guid _categoryId;

    public CreateProductCommandHandlerTests()
    {
        _tenantId = Guid.NewGuid();
        _categoryId = Guid.NewGuid();
        _dbContext = CatalogTestFixtures.CreateDbContext(_tenantId);
        _tenantContext = CatalogTestFixtures.CreateTenantContext(_tenantId);
        _currentUserService = CatalogTestFixtures.CreateCurrentUserService();

        _productRepository = Substitute.For<IProductRepository>();
        _categoryRepository = Substitute.For<ICategoryRepository>();

        _handler = new CreateProductCommandHandler(
            _productRepository,
            _categoryRepository,
            _dbContext,
            Substitute.For<ILogger<CreateProductCommandHandler>>(),
            _tenantContext,
            _currentUserService);
    }

    [Fact]
    public async Task Handle_ValidCommand_CreatesProduct()
    {
        // Arrange
        var category = CatalogTestFixtures.CreateTestCategory(_tenantId);
        _categoryRepository.GetByIdAsync(_categoryId, Arg.Any<CancellationToken>())
            .Returns(category);
        _productRepository.SkuExistsAsync(Arg.Any<string>(), null, Arg.Any<CancellationToken>())
            .Returns(false);

        var command = new CreateProductCommand(
            CategoryId: _categoryId,
            Name: "Test Product",
            Sku: "TEST-001");

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result.ProductId.Should().NotBeEmpty();
        result.Name.Should().Be("Test Product");
        result.Sku.Should().Be("TEST-001");
    }

    [Fact]
    public async Task Handle_CategoryNotFound_ThrowsException()
    {
        // Arrange
        _categoryRepository.GetByIdAsync(_categoryId, Arg.Any<CancellationToken>())
            .Returns((Category?)null);

        var command = new CreateProductCommand(
            CategoryId: _categoryId,
            Name: "Test",
            Sku: "TEST-002");

        // Act & Assert
        var act = () => _handler.Handle(command, CancellationToken.None);
        await act.Should().ThrowAsync<InvalidOperationException>();
    }

    [Fact]
    public async Task Handle_DuplicateSku_ThrowsException()
    {
        // Arrange
        var category = CatalogTestFixtures.CreateTestCategory(_tenantId);
        _categoryRepository.GetByIdAsync(_categoryId, Arg.Any<CancellationToken>())
            .Returns(category);
        _productRepository.SkuExistsAsync("DUPLICATE", null, Arg.Any<CancellationToken>())
            .Returns(true);

        var command = new CreateProductCommand(
            CategoryId: _categoryId,
            Name: "Duplicate",
            Sku: "DUPLICATE");

        // Act & Assert
        var act = () => _handler.Handle(command, CancellationToken.None);
        await act.Should().ThrowAsync<InvalidOperationException>();
    }

    [Fact]
    public async Task Handle_WithAttributes_CreatesProductWithAttributes()
    {
        // Arrange
        var category = CatalogTestFixtures.CreateTestCategory(_tenantId);
        _categoryRepository.GetByIdAsync(_categoryId, Arg.Any<CancellationToken>())
            .Returns(category);
        _productRepository.SkuExistsAsync(Arg.Any<string>(), null, Arg.Any<CancellationToken>())
            .Returns(false);

        var attributes = new Dictionary<string, string>
        {
            { "Color", "Red" },
            { "Size", "Large" }
        };

        var command = new CreateProductCommand(
            CategoryId: _categoryId,
            Name: "Attributed Product",
            Sku: "ATTR-001",
            Attributes: attributes);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        _productRepository.Received(1).Add(Arg.Any<Product>());
    }

    [Fact]
    public async Task Handle_WithTags_CreatesProductWithTags()
    {
        // Arrange
        var category = CatalogTestFixtures.CreateTestCategory(_tenantId);
        _categoryRepository.GetByIdAsync(_categoryId, Arg.Any<CancellationToken>())
            .Returns(category);
        _productRepository.SkuExistsAsync(Arg.Any<string>(), null, Arg.Any<CancellationToken>())
            .Returns(false);

        var tags = new List<string> { "Sale", "Featured", "New" };

        var command = new CreateProductCommand(
            CategoryId: _categoryId,
            Name: "Tagged Product",
            Sku: "TAG-001",
            Tags: tags);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        _productRepository.Received(1).Add(Arg.Any<Product>());
    }
}
