using MediatR;
using KromicStore.Application.Common.Abstractions;
using KromicStore.Application.Features.Catalog.Abstractions;
using Microsoft.Extensions.Logging;

namespace KromicStore.Application.Features.Catalog.Commands.CreateProductImage;

public sealed class CreateProductImageCommandHandler : IRequestHandler<CreateProductImageCommand, CreateProductImageResponse>
{
    private readonly IProductRepository _productRepository;
    private readonly IApplicationDbContext _dbContext;
    private readonly ILogger<CreateProductImageCommandHandler> _logger;
    private readonly ITenantContext _tenantContext;
    private readonly ICurrentUserService _currentUserService;

    public CreateProductImageCommandHandler(
        IProductRepository productRepository,
        IApplicationDbContext dbContext,
        ILogger<CreateProductImageCommandHandler> logger,
        ITenantContext tenantContext,
        ICurrentUserService currentUserService)
    {
        _productRepository = productRepository ?? throw new ArgumentNullException(nameof(productRepository));
        _dbContext = dbContext ?? throw new ArgumentNullException(nameof(dbContext));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        _tenantContext = tenantContext ?? throw new ArgumentNullException(nameof(tenantContext));
        _currentUserService = currentUserService ?? throw new ArgumentNullException(nameof(currentUserService));
    }

    public async Task<CreateProductImageResponse> Handle(CreateProductImageCommand command, CancellationToken cancellationToken)
    {
        _logger.LogInformation("Creating image for product: {ProductId}", command.ProductId);

        // Get the product
        var product = await _productRepository.GetByIdAsync(command.ProductId, cancellationToken);
        if (product == null)
        {
            _logger.LogWarning("Product not found: {ProductId}", command.ProductId);
            throw new InvalidOperationException($"Product with ID {command.ProductId} not found.");
        }

        // Add image to product
        product.AddImage(
            url: command.ImageUrl,
            altText: command.AltText,
            isPrimary: command.IsPrimary);

        // Get the newly added image
        var image = product.Images.LastOrDefault();
        if (image == null)
        {
            _logger.LogError("Failed to add image to product: {ProductId}", command.ProductId);
            throw new InvalidOperationException($"Failed to add image to product {command.ProductId}.");
        }

        // Save changes
        await _dbContext.SaveChangesAsync(cancellationToken);

        _logger.LogInformation("Image created successfully: {ImageId} for product {ProductId}", image.Id, product.Id);

        return new CreateProductImageResponse(
            ImageId: image.Id,
            ProductId: product.Id,
            ImageUrl: image.Url,
            IsPrimary: image.IsPrimary);
    }
}
