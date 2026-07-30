using MediatR;
using KromicStore.Application.Common.Abstractions;
using KromicStore.Application.Features.Catalog.Abstractions;
using KromicStore.Domain.Catalog.Entities;
using Microsoft.Extensions.Logging;

namespace KromicStore.Application.Features.Catalog.Commands.UpdateCollection;

public sealed class UpdateCollectionCommandHandler : IRequestHandler<UpdateCollectionCommand, UpdateCollectionResponse>
{
    private readonly ICollectionRepository _collectionRepository;
    private readonly IApplicationDbContext _dbContext;
    private readonly ILogger<UpdateCollectionCommandHandler> _logger;
    private readonly ITenantContext _tenantContext;
    private readonly ICurrentUserService _currentUserService;

    public UpdateCollectionCommandHandler(
        ICollectionRepository collectionRepository,
        IApplicationDbContext dbContext,
        ILogger<UpdateCollectionCommandHandler> logger,
        ITenantContext tenantContext,
        ICurrentUserService currentUserService)
    {
        _collectionRepository = collectionRepository ?? throw new ArgumentNullException(nameof(collectionRepository));
        _dbContext = dbContext ?? throw new ArgumentNullException(nameof(dbContext));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        _tenantContext = tenantContext ?? throw new ArgumentNullException(nameof(tenantContext));
        _currentUserService = currentUserService ?? throw new ArgumentNullException(nameof(currentUserService));
    }

    public async Task<UpdateCollectionResponse> Handle(UpdateCollectionCommand command, CancellationToken cancellationToken)
    {
        _logger.LogInformation("Updating collection: {CollectionId}", command.CollectionId);

        // Get the collection
        var collection = await _collectionRepository.GetByIdAsync(command.CollectionId, cancellationToken);
        if (collection == null)
        {
            _logger.LogWarning("Collection not found: {CollectionId}", command.CollectionId);
            throw new InvalidOperationException($"Collection with ID {command.CollectionId} not found.");
        }

        // Check for duplicate name if provided
        if (!string.IsNullOrEmpty(command.Name))
        {
            var nameExists = await _collectionRepository.NameExistsAsync(command.Name, command.CollectionId, cancellationToken);
            if (nameExists)
            {
                _logger.LogWarning("Duplicate collection name attempted: {Name}", command.Name);
                throw new InvalidOperationException($"A collection with name '{command.Name}' already exists.");
            }
        }

        // Parse status enum if provided
        CollectionStatus? status = null;
        if (!string.IsNullOrEmpty(command.Status))
        {
            status = Enum.Parse<CollectionStatus>(command.Status);
        }

        // Update the collection
        collection.Update(
            name: command.Name,
            description: command.Description,
            displayOrder: command.DisplayOrder,
            status: status);

        // Mark as modified
        collection.MarkModified(DateTime.UtcNow, _currentUserService.UserId.ToString());

        // Save changes
        await _dbContext.SaveChangesAsync(cancellationToken);

        _logger.LogInformation("Collection updated successfully: {CollectionId}", collection.Id);

        return new UpdateCollectionResponse(
            CollectionId: collection.Id,
            Name: collection.Name,
            Status: collection.Status.ToString());
    }
}
