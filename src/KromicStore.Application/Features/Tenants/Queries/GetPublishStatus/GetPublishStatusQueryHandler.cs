using MediatR;
using Microsoft.Extensions.Logging;
using KromicStore.Application.Features.Tenants.Abstractions;

namespace KromicStore.Application.Features.Tenants.Queries.GetPublishStatus;

public sealed class GetPublishStatusQueryHandler : IRequestHandler<GetPublishStatusQuery, PublishStatusResponse>
{
    private readonly ITenantRepository _tenantRepository;
    private readonly ILogger<GetPublishStatusQueryHandler> _logger;

    public GetPublishStatusQueryHandler(ITenantRepository tenantRepository, ILogger<GetPublishStatusQueryHandler> logger)
    {
        _tenantRepository = tenantRepository ?? throw new ArgumentNullException(nameof(tenantRepository));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    public async Task<PublishStatusResponse> Handle(GetPublishStatusQuery request, CancellationToken cancellationToken)
    {
        _logger.LogInformation("Retrieving publish status for tenant {TenantId}", request.TenantId);

        var tenant = await _tenantRepository.GetByIdAsync(request.TenantId, cancellationToken);
        if (tenant == null)
            throw new InvalidOperationException($"Tenant {request.TenantId} not found.");

        var isPublished = tenant.Status == KromicStore.Domain.Tenants.TenantStatus.Active;
        
        // Get primary domain if available - could be custom or subdomain
        var primaryDomain = tenant.Domains.FirstOrDefault(d => d.IsPrimary);
        var storeUrl = primaryDomain != null 
            ? $"https://{(primaryDomain.CustomDomain ?? primaryDomain.Subdomain)}"
            : $"https://{tenant.Slug}.{GetDomain()}";

        return new PublishStatusResponse
        {
            IsPublished = isPublished,
            PublishedOnUtc = tenant.CreatedOnUtc,
            StoreUrl = storeUrl,
            Status = isPublished ? "Published" : "Draft"
        };
    }

    private static string GetDomain() => Environment.GetEnvironmentVariable("STORE_DOMAIN") ?? "kromicstore.com";
}