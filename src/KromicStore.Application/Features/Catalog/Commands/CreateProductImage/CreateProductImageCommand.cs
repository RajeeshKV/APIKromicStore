using MediatR;

namespace KromicStore.Application.Features.Catalog.Commands.CreateProductImage;

public sealed record CreateProductImageCommand(
    Guid ProductId,
    string ImageUrl,
    string? AltText = null,
    bool IsPrimary = false) : IRequest<CreateProductImageResponse>;

public sealed record CreateProductImageResponse(
    Guid ImageId,
    Guid ProductId,
    string ImageUrl,
    bool IsPrimary);
