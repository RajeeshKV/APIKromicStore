using MediatR;
using KromicStore.Application.Common.Abstractions;
using KromicStore.Application.Features.Shopping.Abstractions;
using KromicStore.Domain.Shopping.Entities;
using Microsoft.Extensions.Logging;

namespace KromicStore.Application.Features.Shopping.Commands.CreateCart;

/// <summary>
/// Handler for CreateCart command.
/// Creates a new shopping cart for either an authenticated customer or a guest.
/// </summary>
public sealed class CreateCartCommandHandler : IRequestHandler<CreateCartCommand, CreateCartResponse>
{
    private readonly ICartRepository _cartRepository;
    private readonly IApplicationDbContext _dbContext;
    private readonly ILogger<CreateCartCommandHandler> _logger;
    private readonly ITenantContext _tenantContext;
    private readonly ICurrentUserService _currentUserService;

    public CreateCartCommandHandler(
        ICartRepository cartRepository,
        IApplicationDbContext dbContext,
        ILogger<CreateCartCommandHandler> logger,
        ITenantContext tenantContext,
        ICurrentUserService currentUserService)
    {
        _cartRepository = cartRepository ?? throw new ArgumentNullException(nameof(cartRepository));
        _dbContext = dbContext ?? throw new ArgumentNullException(nameof(dbContext));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        _tenantContext = tenantContext ?? throw new ArgumentNullException(nameof(tenantContext));
        _currentUserService = currentUserService ?? throw new ArgumentNullException(nameof(currentUserService));
    }

    public async Task<CreateCartResponse> Handle(CreateCartCommand command, CancellationToken cancellationToken)
    {
        var tenantId = _tenantContext.TenantId ?? throw new InvalidOperationException("Tenant context is not resolved");

        // Determine if this is a customer or guest cart
        Cart cart;

        if (command.CustomerId.HasValue && command.CustomerId != Guid.Empty)
        {
            _logger.LogInformation("Creating cart for customer: {CustomerId}", command.CustomerId);

            // Check if customer already has an active cart
            var existingCart = await _cartRepository.GetByCustomerIdAsync(command.CustomerId.Value, cancellationToken);
            if (existingCart != null)
            {
                _logger.LogWarning("Customer already has an active cart: {CustomerId}", command.CustomerId);
                throw new InvalidOperationException("Customer already has an active cart");
            }

            cart = Cart.CreateForCustomer(tenantId, command.CustomerId.Value, command.Currency);
        }
        else if (!string.IsNullOrWhiteSpace(command.AnonymousSessionId))
        {
            _logger.LogInformation("Creating guest cart for session: {SessionId}", command.AnonymousSessionId);

            // Check if guest already has an active cart
            var existingCart = await _cartRepository.GetByAnonymousSessionIdAsync(command.AnonymousSessionId, cancellationToken);
            if (existingCart != null)
            {
                _logger.LogWarning("Guest already has an active cart: {SessionId}", command.AnonymousSessionId);
                throw new InvalidOperationException("Guest already has an active cart for this session");
            }

            cart = Cart.CreateForGuest(tenantId, command.AnonymousSessionId, command.Currency);
        }
        else
        {
            throw new InvalidOperationException("Either CustomerId or AnonymousSessionId must be provided");
        }

        _cartRepository.Add(cart);
        await _dbContext.SaveChangesAsync(cancellationToken);

        _logger.LogInformation("Cart created successfully: {CartId}", cart.Id);

        return new CreateCartResponse(
            CartId: cart.Id,
            CustomerId: cart.CustomerId,
            AnonymousSessionId: cart.AnonymousSessionId,
            Currency: cart.Currency,
            ItemsCount: cart.GetItemsCount(),
            SubTotal: cart.GetSubtotal());
    }
}
