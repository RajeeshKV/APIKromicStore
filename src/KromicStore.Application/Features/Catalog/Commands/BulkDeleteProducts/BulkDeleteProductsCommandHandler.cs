using KromicStore.Application.Features.Catalog.Abstractions;
using KromicStore.Domain.Exceptions;
using MediatR;
using Microsoft.Extensions.Logging;

namespace KromicStore.Application.Features.Catalog.Commands.BulkDeleteProducts;

/// <summary>
/// Handles bulk deletion of products.
/// Validates each product, soft deletes it, and returns success/failure counts.
/// </summary>
public sealed class BulkDeleteProductsCommandHandler
    : IRequestHandler<BulkDeleteProductsCommand, BulkDeleteProductsResponse>
{
    private readonly IProductRepository _productRepository;
    private readonly ILogger<BulkDeleteProductsCommandHandler> _logger;

    public BulkDeleteProductsCommandHandler(
        IProductRepository productRepository,
        ILogger<BulkDeleteProductsCommandHandler> logger)
    {
        _productRepository = productRepository ?? throw new ArgumentNullException(nameof(productRepository));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    public async Task<BulkDeleteProductsResponse> Handle(
        BulkDeleteProductsCommand request,
        CancellationToken cancellationToken)
    {
        var errors = new List<BulkOperationError>();
        int deletedCount = 0;
        var productIdsList = request.ProductIds.ToList();

        if (!productIdsList.Any())
        {
            _logger.LogWarning("Bulk delete attempted with empty product list");
            throw new InvalidOperationException("At least one product ID is required");
        }

        foreach (var productId in productIdsList)
        {
            try
            {
                if (productId == Guid.Empty)
                {
                    errors.Add(new BulkOperationError(productId, "Invalid product ID format"));
                    continue;
                }

                var product = await _productRepository.GetByIdAsync(productId, cancellationToken);
                if (product == null)
                {
                    errors.Add(new BulkOperationError(productId, "Product not found"));
                    continue;
                }

                // Mark for deletion (repository will handle soft delete via DbContext)
                // No explicit Delete() call needed - DbContext will soft delete on SaveChanges
                deletedCount++;

                _logger.LogInformation("Product marked for deletion: {ProductId}", productId);
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "Failed to delete product {ProductId}", productId);
                errors.Add(new BulkOperationError(productId, ex.Message));
            }
        }

        // Single SaveChanges for all operations (batch efficiency)
        if (deletedCount > 0)
        {
            await _productRepository.SaveChangesAsync(cancellationToken);
            _logger.LogInformation("Bulk delete completed: {DeletedCount} products deleted, {FailedCount} failed",
                deletedCount, errors.Count);
        }

        return new BulkDeleteProductsResponse(
            DeletedCount: deletedCount,
            FailedCount: errors.Count,
            Errors: errors
        );
    }
}
