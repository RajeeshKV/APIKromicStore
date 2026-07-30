using MediatR;

namespace KromicStore.Application.Features.Catalog.Commands.UpdateCollection;

public sealed record UpdateCollectionCommand(
    Guid CollectionId,
    string? Name = null,
    string? Description = null,
    int? DisplayOrder = null,
    string? Status = null) : IRequest<UpdateCollectionResponse>;

public sealed record UpdateCollectionResponse(
    Guid CollectionId,
    string Name,
    string Status);
