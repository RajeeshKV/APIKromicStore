using MediatR;

namespace KromicStore.Application.Features.Catalog.Commands.DeleteProduct;

public sealed record DeleteProductCommand(
    Guid ProductId) : IRequest<DeleteProductResponse>;

public sealed record DeleteProductResponse(
    Guid ProductId,
    string Message);
