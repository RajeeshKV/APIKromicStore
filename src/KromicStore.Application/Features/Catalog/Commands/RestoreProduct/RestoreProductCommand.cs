using MediatR;

namespace KromicStore.Application.Features.Catalog.Commands.RestoreProduct;

public sealed record RestoreProductCommand(
    Guid ProductId) : IRequest<RestoreProductResponse>;

public sealed record RestoreProductResponse(
    Guid ProductId,
    string Message);
