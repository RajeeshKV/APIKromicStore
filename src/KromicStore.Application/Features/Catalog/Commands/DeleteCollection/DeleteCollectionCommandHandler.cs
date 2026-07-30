using MediatR;
using KromicStore.Application.Common.Abstractions;
using KromicStore.Application.Features.Catalog.Abstractions;
using Microsoft.Extensions.Logging;

namespace KromicStore.Application.Features.Catalog.Commands.DeleteCollection;

public sealed class DeleteCollectionCommandHandler : IRequestHandler<DeleteCollectionCommand, DeleteCollectionResponse>
{
    private readonly ICollectionRepository _collectionRepository;
    private readonly IApplicationDbContext _dbContext;
    private readonly ILogger<DeleteCollectionCommandHandler> _logger;
    private readonly ITenantContext _tenantContext;
    private readonly ICurrentUserService _currentUserService;

    public DeleteCollectionCommandHandler(
        ICollectionRepository collectionRepository,
        IApplicationDbContext dbContext,
        ILogger<DeleteCollectionCommandHandler> logger,
        ITenantContext tenantContext,
        ICurrentUserService currentUserService)
    {
        _collectionRepository = collectionRepository ?? throw new ArgumentNullException(nameof(collectionRepository));
        _dbContext = dbContext ?? throw new ArgumentNullException(nameof(dbContext));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        _tenantContext = tenantContext ?? throw new ArgumentNullException(nameof(tenantContext));
        _currentUserService = currentUserService ?? throw new ArgumentNullException(nameof(currentUserService));
    }

    public async Task<DeleteCollectionResponse> Handle(DeleteCollectionCommand command, CancellationToken cancellationToken)
    {
        _logger.LogInformation("Deleting collection: {CollectionId}", command.CollectionId);

        // Get the collection
        var collection = await _collectionRepository.GetByIdAsync(command.CollectionId, cancellationToken);
        if (collection == null)
        {
            _logger.LogWarning("Collection not found: {CollectionId}", command.CollectionId);
            throw new InvalidOperationException($"Collection with ID {command.CollectionId} not found.");
        }

        // Soft delete the collection
        collection.SoftDelete(DateTime.UtcNow, _currentUserService.UserId.ToString());

        // Save changes
        await _dbContext.SaveChangesAsync(cancellationToken);

        _logger.LogInformation("Collection deleted successfully: {CollectionId}", collection.Id);

        return new DeleteCollectionResponse(
            CollectionId: collection.Id,
            Message: "Collection has been deleted successfully.");
    }
}
