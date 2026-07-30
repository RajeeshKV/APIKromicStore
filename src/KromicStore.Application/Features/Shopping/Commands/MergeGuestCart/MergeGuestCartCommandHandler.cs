using MediatR;
using KromicStore.Application.Common.Abstractions;
using KromicStore.Application.Features.Shopping.Abstractions;
using Microsoft.Extensions.Logging;

namespace KromicStore.Application.Features.Shopping.Commands.MergeGuestCart;

/// <summary>
/// Handler for MergeGuestCart command.
/// Merges a guest cart into a customer's active cart when they log in.
/// Handles conflict resolution (same product in both carts).
/// </summary>
public sealed class MergeGuestCartCommandHandler : IRequestHandler<MergeGuestCartCommand, MergeGuestCartResponse>
{
    private readonly ICartRepository _cartRepository;
    private readonly IApplicationDbContext _dbContext;
    private readonly ILogger<MergeGuestCartCommandHandler> _logger;
    private readonly ITenantContext _tenantContext;

    public MergeGuestCartCommandHandler(
        ICartRepository cartRepository,
        IApplicationDbContext dbContext,
        ILogger<MergeGuestCartCommandHandler> logger,
        ITenantContext tenantContext)
    {
        _cartRepository = cartRepository ?? throw new ArgumentNullException(nameof(cartRepository));
        _dbContext = dbContext ?? throw new ArgumentNullException(nameof(dbContext));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        _tenantContext = tenantContext ?? throw new ArgumentNullException(nameof(tenantContext));
    }

    public async Task<MergeGuestCartResponse> Handle(MergeGuestCartCommand command, CancellationToken cancellationToken)
    {
        _logger.LogInformation("Merging guest cart {AnonymousSessionId} to customer {CustomerId}", 
            command.AnonymousSessionId, command.CustomerId);

        var tenantId = _tenantContext.TenantId ?? throw new InvalidOperationException("Tenant context is not resolved");

        // Get guest cart by anonymous session ID
        var guestCart = await _cartRepository.GetByAnonymousSessionIdAsync(command.AnonymousSessionId, cancellationToken);
        if (guestCart == null)
        {
            _logger.LogInformation("No guest cart found for session: {AnonymousSessionId}", command.AnonymousSessionId);
            // No guest cart to merge, return early
            return new MergeGuestCartResponse(
                MergedCartId: Guid.Empty,
                CustomerId: command.CustomerId,
                ItemsMerged: 0,
                ItemsInCustomerCart: 0,
                TotalItems: 0,
                MergedSubTotal: 0,
                Status: "NoGuestCartFound");
        }

        if (guestCart.TenantId != tenantId)
        {
            _logger.LogWarning("Unauthorized access to guest cart: {AnonymousSessionId}", command.AnonymousSessionId);
            throw new UnauthorizedAccessException("Cannot access guest cart from another tenant");
        }

        // Get or create customer cart
        var customerCart = await _cartRepository.GetByCustomerIdAsync(command.CustomerId, cancellationToken);
        
        if (customerCart == null)
        {
            _logger.LogInformation("Creating new cart for customer: {CustomerId}", command.CustomerId);
            // Convert guest cart to customer cart
            guestCart.ConvertToCustomerCart(command.CustomerId);
            await _dbContext.SaveChangesAsync(cancellationToken);

            _logger.LogInformation("Guest cart converted to customer cart: {CartId}", guestCart.Id);

            return new MergeGuestCartResponse(
                MergedCartId: guestCart.Id,
                CustomerId: command.CustomerId,
                ItemsMerged: guestCart.GetItemsCount(),
                ItemsInCustomerCart: 0,
                TotalItems: guestCart.GetItemsCount(),
                MergedSubTotal: guestCart.GetSubtotal(),
                Status: "Converted");
        }

        // Merge guest cart items into customer cart
        int itemsMerged = 0;
        foreach (var guestItem in guestCart.Items)
        {
            // Check if item already exists in customer cart
            bool itemExists = customerCart.HasItem(guestItem.ProductId, guestItem.ProductVariantId);
            
            if (itemExists)
            {
                // Update quantity (add to existing)
                var existingItem = customerCart.Items.FirstOrDefault(i => 
                    i.ProductId == guestItem.ProductId && i.ProductVariantId == guestItem.ProductVariantId);
                
                if (existingItem != null)
                {
                    int newQuantity = existingItem.Quantity + guestItem.Quantity;
                    customerCart.UpdateItemQuantity(guestItem.ProductId, newQuantity, guestItem.ProductVariantId);
                    _logger.LogInformation("Updated quantity for product {ProductId} in customer cart", guestItem.ProductId);
                    itemsMerged++;
                }
            }
            else
            {
                // Add new item to customer cart
                customerCart.AddItem(guestItem.ProductId, guestItem.UnitPrice, guestItem.Quantity, guestItem.ProductVariantId);
                _logger.LogInformation("Added product {ProductId} to customer cart", guestItem.ProductId);
                itemsMerged++;
            }
        }

        // Delete guest cart after merge
        guestCart.Delete();
        await _dbContext.SaveChangesAsync(cancellationToken);

        _logger.LogInformation("Guest cart merged successfully. Items merged: {ItemsMerged}", itemsMerged);

        return new MergeGuestCartResponse(
            MergedCartId: customerCart.Id,
            CustomerId: command.CustomerId,
            ItemsMerged: itemsMerged,
            ItemsInCustomerCart: customerCart.GetItemsCount(),
            TotalItems: customerCart.GetItemsCount(),
            MergedSubTotal: customerCart.GetSubtotal(),
            Status: "Merged");
    }
}
