using MediatR;

namespace KromicStore.Application.Features.Catalog.Commands.DuplicateProduct;

public sealed record DuplicateProductCommand(
    Guid ProductId,
    string NewSku,
    string NewName,
    string? NewSlug = null) : IRequest<DuplicateProductResponse>;

public sealed record DuplicateProductResponse(
    Guid DuplicatedProductId,
    string NewSku,
    string NewName,
    string NewSlug);
