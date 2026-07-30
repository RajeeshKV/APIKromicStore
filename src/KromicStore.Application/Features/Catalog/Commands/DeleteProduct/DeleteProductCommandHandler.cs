using MediatR;
using KromicStore.Application.Common.Abstractions;
using KromicStore.Application.Features.Catalog.Abstractions;
using Microsoft.Extensions.Logging;

namespace KromicStore.Application.Features.Catalog.Commands.DeleteProduct;

public sealed class DeleteProductCommandHandler : IRequestHandler<DeleteProductCommand, DeleteProductResponse>
{
    private readonly IProductRepository _productRepository;
    private readonly IApplicationDbContext _dbContext;
    private readonly ILogger<DeleteProductCommandHandler> _logger;
    private readonly ITenantContext _tenantContext;
    private readonly ICurrentUserService _currentUserService;

    public DeleteProductCommandHandler(
        IProductRepository productRepository,
        IApplicationDbContext dbContext,
        ILogger<DeleteProductCommandHandler> logger,
        ITenantContext tenantContext,
        ICurrentUserService currentUserService)
    {
        _productRepository = productRepository ?? throw new ArgumentNullException(nameof(productRepository));
        _dbContext = dbContext ?? throw new ArgumentNullException(nameof(dbContext));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        _tenantContext = tenantContext ?? throw new ArgumentNullException(nameof(tenantContext));
        _currentUserService = currentUserService ?? throw new ArgumentNullException(nameof(currentUserService));
    }

    public async Task<DeleteProductResponse> Handle(DeleteProductCommand command, CancellationToken cancellationToken)
    {
        _logger.LogInformation("Deleting product: {ProductId}", command.ProductId);

        // Get the product
        var product = await _productRepository.GetByIdAsync(command.ProductId, cancellationToken);
        if (product == null)
        {
            _logger.LogWarning("Product not found: {ProductId}", command.ProductId);
            throw new InvalidOperationException($"Product with ID {command.ProductId} not found.");
        }

        // Soft delete the product
        product.SoftDelete(DateTime.UtcNow, _currentUserService.UserId.ToString());

        // Save changes
        await _dbContext.SaveChangesAsync(cancellationToken);

        _logger.LogInformation("Product deleted successfully: {ProductId}", product.Id);

        return new DeleteProductResponse(
            ProductId: product.Id,
            Message: "Product has been deleted successfully.");
    }
}
