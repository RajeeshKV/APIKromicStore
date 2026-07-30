using MediatR;

namespace KromicStore.Application.Features.Catalog.Commands.DeleteProductImage;

public sealed record DeleteProductImageCommand(
    Guid ProductId,
    Guid ImageId) : IRequest<DeleteProductImageResponse>;

public sealed record DeleteProductImageResponse(
    Guid ImageId,
    string Message);
