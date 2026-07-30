using MediatR;
using KromicStore.Application.Common.Abstractions;
using KromicStore.Application.Features.Catalog.Abstractions;
using Microsoft.Extensions.Logging;

namespace KromicStore.Application.Features.Catalog.Commands.DeleteProductImage;

public sealed class DeleteProductImageCommandHandler : IRequestHandler<DeleteProductImageCommand, DeleteProductImageResponse>
{
    private readonly IProductRepository _productRepository;
    private readonly IApplicationDbContext _dbContext;
    private readonly ILogger<DeleteProductImageCommandHandler> _logger;
    private readonly ITenantContext _tenantContext;
    private readonly ICurrentUserService _currentUserService;

    public DeleteProductImageCommandHandler(
        IProductRepository productRepository,
        IApplicationDbContext dbContext,
        ILogger<DeleteProductImageCommandHandler> logger,
        ITenantContext tenantContext,
        ICurrentUserService currentUserService)
    {
        _productRepository = productRepository ?? throw new ArgumentNullException(nameof(productRepository));
        _dbContext = dbContext ?? throw new ArgumentNullException(nameof(dbContext));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        _tenantContext = tenantContext ?? throw new ArgumentNullException(nameof(tenantContext));
        _currentUserService = currentUserService ?? throw new ArgumentNullException(nameof(currentUserService));
    }

    public async Task<DeleteProductImageResponse> Handle(DeleteProductImageCommand command, CancellationToken cancellationToken)
    {
        _logger.LogInformation("Deleting image: {ImageId} from product {ProductId}", command.ImageId, command.ProductId);

        // Get the product
        var product = await _productRepository.GetByIdAsync(command.ProductId, cancellationToken);
        if (product == null)
        {
            _logger.LogWarning("Product not found: {ProductId}", command.ProductId);
            throw new InvalidOperationException($"Product with ID {command.ProductId} not found.");
        }

        // Get the image from the product
        var image = product.Images.FirstOrDefault(i => i.Id == command.ImageId);
        if (image == null)
        {
            _logger.LogWarning("Image not found: {ImageId} in product {ProductId}", command.ImageId, command.ProductId);
            throw new InvalidOperationException($"Image with ID {command.ImageId} not found in product {command.ProductId}.");
        }

        // Remove image from product
        product.RemoveImage(command.ImageId);

        // Save changes
        await _dbContext.SaveChangesAsync(cancellationToken);

        _logger.LogInformation("Image deleted successfully: {ImageId}", command.ImageId);

        return new DeleteProductImageResponse(
            ImageId: command.ImageId,
            Message: "Image has been deleted successfully.");
    }
}
