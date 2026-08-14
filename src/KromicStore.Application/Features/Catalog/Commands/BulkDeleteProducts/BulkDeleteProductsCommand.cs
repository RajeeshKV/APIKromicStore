using MediatR;

namespace KromicStore.Application.Features.Catalog.Commands.BulkDeleteProducts;

/// <summary>
/// Command to bulk delete multiple products.
/// Soft deletes all specified products in a single operation.
/// </summary>
public sealed record BulkDeleteProductsCommand(
    IEnumerable<Guid> ProductIds
) : IRequest<BulkDeleteProductsResponse>;

/// <summary>
/// Response from bulk delete operation with success/failure counts.
/// </summary>
public sealed record BulkDeleteProductsResponse(
    int DeletedCount,
    int FailedCount,
    List<BulkOperationError> Errors
);

/// <summary>
/// Error details for individual items that failed in bulk operation.
/// </summary>
public sealed record BulkOperationError(
    Guid Id,
    string ErrorMessage
);
