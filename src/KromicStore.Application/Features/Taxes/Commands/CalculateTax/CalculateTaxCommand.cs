using MediatR;

namespace KromicStore.Application.Features.Taxes.Commands.CalculateTax;

public sealed class CalculateTaxCommand : IRequest<CalculateTaxResponse>
{
    public Guid TaxRegionId { get; set; }
    public string ProductCategory { get; set; } = string.Empty;
    public decimal OrderAmount { get; set; }
}

public sealed class CalculateTaxResponse
{
    public bool Success { get; set; }
    public string Message { get; set; } = string.Empty;
    public decimal TaxAmount { get; set; }
    public decimal TaxRate { get; set; }
}
