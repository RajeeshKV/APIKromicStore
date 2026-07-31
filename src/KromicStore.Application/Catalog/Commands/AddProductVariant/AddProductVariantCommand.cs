using MediatR;

namespace KromicStore.Application.Catalog.Commands.AddProductVariant;

/// <summary>
/// Command to add a variant to an existing product.
/// </summary>
public sealed class AddProductVariantCommand : IRequest<AddProductVariantResponse>
{
    public Guid ProductId { get; set; }
    public string Sku { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public decimal Price { get; set; }
    public int Quantity { get; set; }
    public Dictionary<string, string> Attributes { get; set; } = new();
    public bool IsActive { get; set; } = true;
}

public sealed class AddProductVariantResponse
{
    public Guid ProductId { get; set; }
    public Guid VariantId { get; set; }
    public string Sku { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public decimal Price { get; set; }
    public DateTime CreatedOnUtc { get; set; }
}
