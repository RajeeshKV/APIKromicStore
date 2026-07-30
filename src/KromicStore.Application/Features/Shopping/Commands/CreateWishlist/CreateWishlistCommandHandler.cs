using MediatR;
using KromicStore.Application.Common.Abstractions;
using KromicStore.Application.Features.Shopping.Abstractions;
using KromicStore.Domain.Shopping.Entities;
using Microsoft.Extensions.Logging;

namespace KromicStore.Application.Features.Shopping.Commands.CreateWishlist;

/// <summary>
/// Handler for CreateWishlist command.
/// Creates a new wishlist for a customer.
/// </summary>
public sealed class CreateWishlistCommandHandler : IRequestHandler<CreateWishlistCommand, CreateWishlistResponse>
{
    private readonly IWishlistRepository _wishlistRepository;
    private readonly IApplicationDbContext _dbContext;
    private readonly ILogger<CreateWishlistCommandHandler> _logger;
    private readonly ITenantContext _tenantContext;
    private readonly ICurrentUserService _currentUserService;

    public CreateWishlistCommandHandler(
        IWishlistRepository wishlistRepository,
        IApplicationDbContext dbContext,
        ILogger<CreateWishlistCommandHandler> logger,
        ITenantContext tenantContext,
        ICurrentUserService currentUserService)
    {
        _wishlistRepository = wishlistRepository ?? throw new ArgumentNullException(nameof(wishlistRepository));
        _dbContext = dbContext ?? throw new ArgumentNullException(nameof(dbContext));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        _tenantContext = tenantContext ?? throw new ArgumentNullException(nameof(tenantContext));
        _currentUserService = currentUserService ?? throw new ArgumentNullException(nameof(currentUserService));
    }

    public async Task<CreateWishlistResponse> Handle(CreateWishlistCommand command, CancellationToken cancellationToken)
    {
        _logger.LogInformation("Creating wishlist for customer: {CustomerId}", command.CustomerId);

        var tenantId = _tenantContext.TenantId ?? throw new InvalidOperationException("Tenant context is not resolved");

        // Check if customer already has a wishlist
        var existingWishlist = await _wishlistRepository.GetByCustomerIdAsync(command.CustomerId, cancellationToken);
        if (existingWishlist != null)
        {
            _logger.LogWarning("Customer already has a wishlist: {CustomerId}", command.CustomerId);
            throw new InvalidOperationException("Customer already has a wishlist");
        }

        // Create new wishlist
        var wishlist = Wishlist.Create(tenantId, command.CustomerId);

        _wishlistRepository.Add(wishlist);
        await _dbContext.SaveChangesAsync(cancellationToken);

        _logger.LogInformation("Wishlist created successfully: {WishlistId}", wishlist.Id);

        return new CreateWishlistResponse(
            WishlistId: wishlist.Id,
            CustomerId: wishlist.CustomerId,
            ItemsCount: wishlist.GetItemsCount());
    }
}
