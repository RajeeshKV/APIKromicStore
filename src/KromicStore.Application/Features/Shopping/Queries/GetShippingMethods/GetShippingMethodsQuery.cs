using MediatR;

namespace KromicStore.Application.Features.Shopping.Queries.GetShippingMethods;

/// <summary>
/// Query to retrieve available shipping methods.
/// In a real implementation, this would filter by country/region and order weight.
/// </summary>
public sealed record GetShippingMethodsQuery(string? CountryCode = null) : IRequest<GetShippingMethodsResponse>;

/// <summary>
/// DTO for a shipping method.
/// </summary>
public sealed record ShippingMethodDto(
    string ShippingMethodId,
    string Name,
    string Description,
    decimal Cost,
    int EstimatedDaysMin,
    int EstimatedDaysMax);

/// <summary>
/// Response for GetShippingMethods query.
/// </summary>
public sealed record GetShippingMethodsResponse(
    List<ShippingMethodDto> ShippingMethods,
    int Count);
