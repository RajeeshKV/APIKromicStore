using MediatR;

namespace KromicStore.Application.Features.Catalog.Commands.DeleteVariant;

public sealed record DeleteVariantCommand(
    Guid ProductId,
    Guid VariantId) : IRequest<DeleteVariantResponse>;

public sealed record DeleteVariantResponse(
    Guid VariantId,
    string Message);
