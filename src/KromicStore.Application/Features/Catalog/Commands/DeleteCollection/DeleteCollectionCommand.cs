using MediatR;

namespace KromicStore.Application.Features.Catalog.Commands.DeleteCollection;

public sealed record DeleteCollectionCommand(
    Guid CollectionId) : IRequest<DeleteCollectionResponse>;

public sealed record DeleteCollectionResponse(
    Guid CollectionId,
    string Message);
