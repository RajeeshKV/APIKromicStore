using MediatR;
using KromicStore.Application.Common.Abstractions;
using KromicStore.Application.Features.Tenants.Abstractions;
using Microsoft.Extensions.Logging;

namespace KromicStore.Application.Features.Storefront.Queries.GetStoreInfo;

public sealed class GetStoreInfoQueryHandler : IRequestHandler<GetStoreInfoQuery, GetStoreInfoResponse>
{
    private readonly ITenantRepository _tenantRepository;
    private readonly ITenantContext _tenantContext;
    private readonly ILogger<GetStoreInfoQueryHandler> _logger;

    public GetStoreInfoQueryHandler(
        ITenantRepository tenantRepository,
        ITenantContext tenantContext,
        ILogger<GetStoreInfoQueryHandler> logger)
    {
        _tenantRepository = tenantRepository ?? throw new ArgumentNullException(nameof(tenantRepository));
        _tenantContext = tenantContext ?? throw new ArgumentNullException(nameof(tenantContext));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    public async Task<GetStoreInfoResponse> Handle(
        GetStoreInfoQuery request,
        CancellationToken cancellationToken)
    {
        if (!_tenantContext.TenantId.HasValue)
        {
            _logger.LogWarning("Tenant not resolved from request");
            throw new InvalidOperationException("No tenant context available");
        }

        _logger.LogInformation("Retrieving store info for tenant {TenantId}", _tenantContext.TenantId);

        var tenant = await _tenantRepository.GetByIdAsync(_tenantContext.TenantId.Value, cancellationToken);

        if (tenant == null)
        {
            _logger.LogWarning("Store not found for tenant {TenantId}", _tenantContext.TenantId);
            throw new InvalidOperationException($"Store not found for tenant {_tenantContext.TenantId}");
        }

        _logger.LogInformation("Retrieved store info for {StoreName}", tenant.StoreName);

        return new GetStoreInfoResponse(
            TenantId: tenant.Id,
            StoreName: tenant.StoreName,
            Description: null,
            LogoUrl: null,
            FaviconUrl: null,
            StoreEmail: null,
            SupportEmail: null,
            PhoneNumber: null,
            CurrencyCode: "USD",
            IsPublished: tenant.Status == KromicStore.Domain.Tenants.TenantStatus.Active);
    }
}
