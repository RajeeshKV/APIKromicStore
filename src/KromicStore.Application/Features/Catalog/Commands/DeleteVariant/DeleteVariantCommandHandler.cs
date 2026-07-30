using MediatR;
using KromicStore.Application.Common.Abstractions;
using KromicStore.Application.Features.Catalog.Abstractions;
using Microsoft.Extensions.Logging;

namespace KromicStore.Application.Features.Catalog.Commands.DeleteVariant;

public sealed class DeleteVariantCommandHandler : IRequestHandler<DeleteVariantCommand, DeleteVariantResponse>
{
    private readonly IProductRepository _productRepository;
    private readonly IApplicationDbContext _dbContext;
    private readonly ILogger<DeleteVariantCommandHandler> _logger;
    private readonly ITenantContext _tenantContext;
    private readonly ICurrentUserService _currentUserService;

    public DeleteVariantCommandHandler(
        IProductRepository productRepository,
        IApplicationDbContext dbContext,
        ILogger<DeleteVariantCommandHandler> logger,
        ITenantContext tenantContext,
        ICurrentUserService currentUserService)
    {
        _productRepository = productRepository ?? throw new ArgumentNullException(nameof(productRepository));
        _dbContext = dbContext ?? throw new ArgumentNullException(nameof(dbContext));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        _tenantContext = tenantContext ?? throw new ArgumentNullException(nameof(tenantContext));
        _currentUserService = currentUserService ?? throw new ArgumentNullException(nameof(currentUserService));
    }

    public async Task<DeleteVariantResponse> Handle(DeleteVariantCommand command, CancellationToken cancellationToken)
    {
        _logger.LogInformation("Deleting variant: {VariantId} from product {ProductId}", command.VariantId, command.ProductId);

        // Get the product
        var product = await _productRepository.GetByIdAsync(command.ProductId, cancellationToken);
        if (product == null)
        {
            _logger.LogWarning("Product not found: {ProductId}", command.ProductId);
            throw new InvalidOperationException($"Product with ID {command.ProductId} not found.");
        }

        // Get the variant from the product
        var variant = product.Variants.FirstOrDefault(v => v.Id == command.VariantId);
        if (variant == null)
        {
            _logger.LogWarning("Variant not found: {VariantId} in product {ProductId}", command.VariantId, command.ProductId);
            throw new InvalidOperationException($"Variant with ID {command.VariantId} not found in product {command.ProductId}.");
        }

        // Remove variant from product
        product.RemoveVariant(command.VariantId);

        // Save changes
        await _dbContext.SaveChangesAsync(cancellationToken);

        _logger.LogInformation("Variant deleted successfully: {VariantId}", command.VariantId);

        return new DeleteVariantResponse(
            VariantId: command.VariantId,
            Message: "Variant has been deleted successfully.");
    }
}
