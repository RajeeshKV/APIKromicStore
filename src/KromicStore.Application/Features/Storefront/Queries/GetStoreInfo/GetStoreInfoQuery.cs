using MediatR;

namespace KromicStore.Application.Features.Storefront.Queries.GetStoreInfo;

/// <summary>
/// Query to retrieve public store information (name, logo, description, etc.)
/// </summary>
public record GetStoreInfoQuery : IRequest<GetStoreInfoResponse>;

public record GetStoreInfoResponse(
    Guid TenantId,
    string StoreName,
    string? Description,
    string? LogoUrl,
    string? FaviconUrl,
    string? StoreEmail,
    string? SupportEmail,
    string? PhoneNumber,
    string? CurrencyCode,
    bool IsPublished);
