using KromicStore.Application.Common.Abstractions;
using KromicStore.Application.Features.Promotions.Abstractions;
using KromicStore.Domain.Promotions.Entities;
using MediatR;

namespace KromicStore.Application.Features.Promotions.Commands.CreateDiscount;

public sealed class CreateDiscountCommandHandler : IRequestHandler<CreateDiscountCommand, CreateDiscountResponse>
{
    private readonly IPromotionRepository _repository;
    private readonly ITenantContext _tenantContext;

    public CreateDiscountCommandHandler(IPromotionRepository repository, ITenantContext tenantContext)
    {
        _repository = repository ?? throw new ArgumentNullException(nameof(repository));
        _tenantContext = tenantContext ?? throw new ArgumentNullException(nameof(tenantContext));
    }

    public async Task<CreateDiscountResponse> Handle(CreateDiscountCommand request, CancellationToken cancellationToken)
    {
        var tenantId = _tenantContext.TenantId ?? throw new InvalidOperationException("Tenant context is required");
        
        Discount discount = request.Type switch
        {
            DiscountType.FixedAmount => Discount.CreateFixedAmountDiscount(
                tenantId, request.Name, request.FixedAmount ?? 0, request.ValidFromUtc, request.ValidToUtc,
                request.Description),
            
            DiscountType.PercentageAmount => Discount.CreatePercentageDiscount(
                tenantId, request.Name, request.PercentageAmount ?? 0, request.ValidFromUtc, request.ValidToUtc,
                request.Description, request.MaxDiscountAmount),
            
            DiscountType.FreeShipping => Discount.CreateFreeShippingDiscount(
                tenantId, request.Name, request.ValidFromUtc, request.ValidToUtc, request.Description),
            
            _ => throw new InvalidOperationException($"Unsupported discount type: {request.Type}")
        };
        
        if (request.MaxUsageCount > 0)
        {
            // MaxUsageCount set via configuration if needed
        }
        
        _repository.AddDiscount(discount);
        await _repository.SaveChangesAsync(cancellationToken);
        
        return new CreateDiscountResponse
        {
            DiscountId = discount.Id,
            Name = discount.Name,
            Type = discount.Type,
            IsActive = discount.IsActive
        };
    }
}
