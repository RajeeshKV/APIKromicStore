using MediatR;
using KromicStore.Application.Common.Abstractions;
using KromicStore.Application.Features.Shopping.Abstractions;
using KromicStore.Domain.Shopping.Entities;
using Microsoft.Extensions.Logging;

namespace KromicStore.Application.Features.Shopping.Commands.CreateCheckoutSession;

/// <summary>
/// Handler for CreateCheckoutSession command.
/// Creates a new checkout session from a shopping cart.
/// </summary>
public sealed class CreateCheckoutSessionCommandHandler : IRequestHandler<CreateCheckoutSessionCommand, CreateCheckoutSessionResponse>
{
    private readonly ICartRepository _cartRepository;
    private readonly ICheckoutSessionRepository _checkoutSessionRepository;
    private readonly IApplicationDbContext _dbContext;
    private readonly ILogger<CreateCheckoutSessionCommandHandler> _logger;
    private readonly ITenantContext _tenantContext;

    public CreateCheckoutSessionCommandHandler(
        ICartRepository cartRepository,
        ICheckoutSessionRepository checkoutSessionRepository,
        IApplicationDbContext dbContext,
        ILogger<CreateCheckoutSessionCommandHandler> logger,
        ITenantContext tenantContext)
    {
        _cartRepository = cartRepository ?? throw new ArgumentNullException(nameof(cartRepository));
        _checkoutSessionRepository = checkoutSessionRepository ?? throw new ArgumentNullException(nameof(checkoutSessionRepository));
        _dbContext = dbContext ?? throw new ArgumentNullException(nameof(dbContext));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        _tenantContext = tenantContext ?? throw new ArgumentNullException(nameof(tenantContext));
    }

    public async Task<CreateCheckoutSessionResponse> Handle(CreateCheckoutSessionCommand command, CancellationToken cancellationToken)
    {
        _logger.LogInformation("Creating checkout session from cart {CartId} for customer {CustomerId}", command.CartId, command.CustomerId);

        var tenantId = _tenantContext.TenantId ?? throw new InvalidOperationException("Tenant context is not resolved");

        // Verify cart exists and belongs to the customer
        var cart = await _cartRepository.GetByIdAsync(command.CartId, cancellationToken);
        if (cart == null)
        {
            _logger.LogWarning("Cart not found: {CartId}", command.CartId);
            throw new InvalidOperationException($"Cart with ID {command.CartId} not found");
        }

        if (cart.TenantId != tenantId)
        {
            _logger.LogWarning("Unauthorized access to cart: {CartId}", command.CartId);
            throw new UnauthorizedAccessException("Cannot access cart from another tenant");
        }

        if (cart.CustomerId != command.CustomerId)
        {
            _logger.LogWarning("Cart does not belong to customer: {CartId}", command.CartId);
            throw new UnauthorizedAccessException("Cart does not belong to the specified customer");
        }

        // Verify cart is not empty
        if (cart.GetItemsCount() == 0)
        {
            _logger.LogWarning("Cannot create checkout session from empty cart: {CartId}", command.CartId);
            throw new InvalidOperationException("Cannot create checkout session from an empty cart");
        }

        // Create checkout session
        var checkoutSession = CheckoutSession.Create(tenantId, command.CustomerId);

        // Copy items from cart
        foreach (var cartItem in cart.Items)
        {
            checkoutSession.AddItem(cartItem.ProductId, cartItem.UnitPrice, cartItem.Quantity, cartItem.ProductVariantId);
        }

        _checkoutSessionRepository.Add(checkoutSession);
        await _dbContext.SaveChangesAsync(cancellationToken);

        _logger.LogInformation("Checkout session created: {CheckoutSessionId}", checkoutSession.Id);

        // Map items
        var items = checkoutSession.Items.Select(i => new CheckoutItemDto(
            ProductId: i.ProductId,
            VariantId: i.ProductVariantId,
            Quantity: i.Quantity,
            UnitPrice: i.UnitPrice,
            LineTotal: i.LineTotal)).ToList();

        return new CreateCheckoutSessionResponse(
            CheckoutSessionId: checkoutSession.Id,
            CustomerId: checkoutSession.CustomerId,
            Currency: "USD",
            Items: items,
            ItemsCount: checkoutSession.Items.Count,
            SubTotal: checkoutSession.SubTotal,
            Status: checkoutSession.Status.ToString(),
            CreatedOnUtc: checkoutSession.CreatedOnUtc);
    }
}
