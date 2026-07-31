using KromicStore.Domain.Promotions.Entities;
using MediatR;

namespace KromicStore.Application.Features.Promotions.Commands.CreateDiscount;

public sealed class CreateDiscountCommand : IRequest<CreateDiscountResponse>
{
    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; }
    public DiscountType Type { get; set; }
    public decimal? FixedAmount { get; set; }
    public decimal? PercentageAmount { get; set; }
    public decimal? MaxDiscountAmount { get; set; }
    public DateTime ValidFromUtc { get; set; }
    public DateTime ValidToUtc { get; set; }
    public int MaxUsageCount { get; set; } = -1; // -1 = unlimited
}

public sealed class CreateDiscountResponse
{
    public Guid DiscountId { get; set; }
    public string Name { get; set; } = string.Empty;
    public DiscountType Type { get; set; }
    public bool IsActive { get; set; }
}
