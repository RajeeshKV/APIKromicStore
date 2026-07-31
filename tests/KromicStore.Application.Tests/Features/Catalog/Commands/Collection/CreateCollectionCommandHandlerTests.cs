using FluentAssertions;
using KromicStore.Application.Common.Abstractions;
using KromicStore.Application.Features.Catalog.Commands.CreateCollection;
using KromicStore.Application.Features.Catalog.Abstractions;
using KromicStore.Application.Tests.Features.Catalog.Common;
using KromicStore.Infrastructure.Persistence;
using Microsoft.Extensions.Logging;
using NSubstitute;
using Xunit;

namespace KromicStore.Application.Tests.Features.Catalog.Commands.Collection;

/// <summary>
/// Handler tests for CreateCollectionCommand.
/// </summary>
public sealed class CreateCollectionCommandHandlerTests
{
    private readonly ICollectionRepository _collectionRepository;
    private readonly ICurrentUserService _currentUserService;
    private readonly KromicStoreDbContext _dbContext;
    private readonly ITenantContext _tenantContext;
    private readonly CreateCollectionCommandHandler _handler;
    private readonly Guid _tenantId;

    public CreateCollectionCommandHandlerTests()
    {
        _tenantId = Guid.NewGuid();
        _dbContext = CatalogTestFixtures.CreateDbContext(_tenantId);
        _tenantContext = CatalogTestFixtures.CreateTenantContext(_tenantId);
        _collectionRepository = Substitute.For<ICollectionRepository>();
        _currentUserService = CatalogTestFixtures.CreateCurrentUserService();
        
        _handler = new CreateCollectionCommandHandler(
            _collectionRepository,
            _dbContext,
            Substitute.For<ILogger<CreateCollectionCommandHandler>>(),
            _tenantContext,
            _currentUserService);
    }

    #region Happy Path

    [Fact]
    public async Task Handle_CreatesCollection_WithValidData()
    {
        // Arrange
        var command = new CreateCollectionCommand(
            Name: "Summer Collection",
            Description: "Summer products",
            DisplayOrder: 1,
            Status: "Active");

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result.CollectionId.Should().NotBe(Guid.Empty);
        result.Name.Should().Be("Summer Collection");
        result.Status.Should().Be("Active");
    }

    [Fact]
    public async Task Handle_CreatesCollection_WithoutDescription()
    {
        // Arrange
        var command = new CreateCollectionCommand(
            Name: "Winter Collection",
            Description: null,
            DisplayOrder: 2,
            Status: "Active");

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result.Name.Should().Be("Winter Collection");
    }

    [Fact]
    public async Task Handle_CreatesCollection_WithDefaultStatus()
    {
        // Arrange
        var command = new CreateCollectionCommand(
            Name: "Default Status",
            Description: null,
            DisplayOrder: 0,
            Status: null);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result.CollectionId.Should().NotBe(Guid.Empty);
    }

    #endregion

    #region Edge Cases

    [Fact]
    public async Task Handle_WithHighDisplayOrder()
    {
        // Arrange
        var command = new CreateCollectionCommand(
            Name: "High Order",
            Description: null,
            DisplayOrder: 9999,
            Status: "Active");

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result.CollectionId.Should().NotBe(Guid.Empty);
    }

    [Fact]
    public async Task Handle_WithLongDescription()
    {
        // Arrange
        var longDesc = string.Concat(Enumerable.Repeat("Description text. ", 50));
        var command = new CreateCollectionCommand(
            Name: "Long Description",
            Description: longDesc,
            DisplayOrder: 1,
            Status: "Active");

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
    }

    [Fact]
    public async Task Handle_MultipleCollections()
    {
        // Arrange
        var command1 = new CreateCollectionCommand(
            Name: "Collection 1",
            Description: null,
            DisplayOrder: 1,
            Status: "Active");

        var command2 = new CreateCollectionCommand(
            Name: "Collection 2",
            Description: null,
            DisplayOrder: 2,
            Status: "Active");

        // Act
        var result1 = await _handler.Handle(command1, CancellationToken.None);
        var result2 = await _handler.Handle(command2, CancellationToken.None);

        // Assert
        result1.CollectionId.Should().NotBe(result2.CollectionId);
    }

    #endregion

    #region Tenant Isolation

    [Fact]
    public async Task Handle_CollectionsIsolatedByTenant()
    {
        // Arrange
        var tenant2Id = Guid.NewGuid();
        var dbContext2 = CatalogTestFixtures.CreateDbContext(tenant2Id);
        var tenantContext2 = CatalogTestFixtures.CreateTenantContext(tenant2Id);
        var collectionRepository2 = Substitute.For<ICollectionRepository>();

        var handler2 = new CreateCollectionCommandHandler(
            collectionRepository2,
            dbContext2,
            Substitute.For<ILogger<CreateCollectionCommandHandler>>(),
            tenantContext2,
            _currentUserService);

        var command = new CreateCollectionCommand(
            Name: "Test",
            Description: null,
            DisplayOrder: 1,
            Status: "Active");

        // Act
        var result1 = await _handler.Handle(command, CancellationToken.None);
        var result2 = await handler2.Handle(command, CancellationToken.None);

        // Assert
        result1.CollectionId.Should().NotBe(result2.CollectionId);
    }

    #endregion
}
