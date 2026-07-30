using MediatR;
using KromicStore.Application.Common.Abstractions;
using KromicStore.Application.Features.Catalog.Abstractions;
using KromicStore.Domain.Catalog.Entities;
using Microsoft.Extensions.Logging;

namespace KromicStore.Application.Features.Catalog.Commands.CreateCollection;

public sealed class CreateCollectionCommandHandler : IRequestHandler<CreateCollectionCommand, CreateCollectionResponse>
{
    private readonly ICollectionRepository _collectionRepository;
    private readonly IApplicationDbContext _dbContext;
    private readonly ILogger<CreateCollectionCommandHandler> _logger;
    private readonly ITenantContext _tenantContext;
    private readonly ICurrentUserService _currentUserService;

    public CreateCollectionCommandHandler(
        ICollectionRepository collectionRepository,
        IApplicationDbContext dbContext,
        ILogger<CreateCollectionCommandHandler> logger,
        ITenantContext tenantContext,
        ICurrentUserService currentUserService)
    {
        _collectionRepository = collectionRepository ?? throw new ArgumentNullException(nameof(collectionRepository));
        _dbContext = dbContext ?? throw new ArgumentNullException(nameof(dbContext));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        _tenantContext = tenantContext ?? throw new ArgumentNullException(nameof(tenantContext));
        _currentUserService = currentUserService ?? throw new ArgumentNullException(nameof(currentUserService));
    }

    public async Task<CreateCollectionResponse> Handle(CreateCollectionCommand command, CancellationToken cancellationToken)
    {
        _logger.LogInformation("Creating collection: {Name}", command.Name);

        // Check for duplicate name
        var nameExists = await _collectionRepository.NameExistsAsync(command.Name, null, cancellationToken);
        if (nameExists)
        {
            _logger.LogWarning("Duplicate collection name attempted: {Name}", command.Name);
            throw new InvalidOperationException($"A collection with name '{command.Name}' already exists.");
        }

        // Parse status enum
        var status = Enum.Parse<CollectionStatus>(command.Status ?? "Active");

        // Create the collection
        var collection = ProductCollection.Create(
            tenantId: _tenantContext.TenantId ?? throw new InvalidOperationException("Tenant context is not resolved"),
            name: command.Name,
            description: command.Description,
            displayOrder: command.DisplayOrder,
            status: status);

        // Mark as created
        collection.MarkCreated(DateTime.UtcNow, _currentUserService.UserId.ToString());

        // Add to repository
        _collectionRepository.Add(collection);

        // Save changes
        await _dbContext.SaveChangesAsync(cancellationToken);

        _logger.LogInformation("Collection created successfully: {CollectionId}", collection.Id);

        return new CreateCollectionResponse(
            CollectionId: collection.Id,
            Name: collection.Name,
            Status: collection.Status.ToString());
    }
}
