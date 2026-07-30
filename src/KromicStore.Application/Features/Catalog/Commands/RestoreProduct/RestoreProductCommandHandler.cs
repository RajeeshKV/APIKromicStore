using MediatR;
using KromicStore.Application.Common.Abstractions;
using KromicStore.Application.Features.Catalog.Abstractions;
using Microsoft.Extensions.Logging;

namespace KromicStore.Application.Features.Catalog.Commands.RestoreProduct;

public sealed class RestoreProductCommandHandler : IRequestHandler<RestoreProductCommand, RestoreProductResponse>
{
    private readonly IProductRepository _productRepository;
    private readonly IApplicationDbContext _dbContext;
    private readonly ILogger<RestoreProductCommandHandler> _logger;
    private readonly ITenantContext _tenantContext;
    private readonly ICurrentUserService _currentUserService;

    public RestoreProductCommandHandler(
        IProductRepository productRepository,
        IApplicationDbContext dbContext,
        ILogger<RestoreProductCommandHandler> logger,
        ITenantContext tenantContext,
        ICurrentUserService currentUserService)
    {
        _productRepository = productRepository ?? throw new ArgumentNullException(nameof(productRepository));
        _dbContext = dbContext ?? throw new ArgumentNullException(nameof(dbContext));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        _tenantContext = tenantContext ?? throw new ArgumentNullException(nameof(tenantContext));
        _currentUserService = currentUserService ?? throw new ArgumentNullException(nameof(currentUserService));
    }

    public async Task<RestoreProductResponse> Handle(RestoreProductCommand command, CancellationToken cancellationToken)
    {
        _logger.LogInformation("Restoring product: {ProductId}", command.ProductId);

        // Get the product (including soft-deleted ones)
        var product = await _productRepository.GetByIdAsync(command.ProductId, cancellationToken);
        if (product == null)
        {
            _logger.LogWarning("Product not found: {ProductId}", command.ProductId);
            throw new InvalidOperationException($"Product with ID {command.ProductId} not found.");
        }

        if (!product.IsDeleted)
        {
            _logger.LogWarning("Product is not deleted: {ProductId}", command.ProductId);
            throw new InvalidOperationException($"Product with ID {command.ProductId} is not deleted.");
        }

        // Restore the product
        product.Restore();

        // Save changes
        await _dbContext.SaveChangesAsync(cancellationToken);

        _logger.LogInformation("Product restored successfully: {ProductId}", product.Id);

        return new RestoreProductResponse(
            ProductId: product.Id,
            Message: "Product has been restored successfully.");
    }
}
