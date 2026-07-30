using MediatR;
using KromicStore.Application.Features.Tenants.Abstractions;

namespace KromicStore.Application.Features.Tenants.Queries.GetTenant;

public sealed class GetTenantQueryHandler : IRequestHandler<GetTenantQuery, GetTenantResponse>
{
    private readonly ITenantRepository _repository;

    public GetTenantQueryHandler(ITenantRepository repository)
    {
        _repository = repository ?? throw new ArgumentNullException(nameof(repository));
    }

    public async Task<GetTenantResponse> Handle(GetTenantQuery request, CancellationToken cancellationToken)
    {
        var tenant = await _repository.GetByIdAsync(request.TenantId, cancellationToken);

        if (tenant is null)
        {
            throw new InvalidOperationException($"Tenant with ID '{request.TenantId}' not found.");
        }

        var domains = tenant.Domains
            .Select(d => new TenantDomainDto(
                Subdomain: d.Subdomain,
                CustomDomain: d.CustomDomain,
                IsPrimary: d.IsPrimary,
                IsVerified: d.IsVerified))
            .ToList()
            .AsReadOnly();

        return new GetTenantResponse(
            TenantId: tenant.Id,
            Name: tenant.Name,
            StoreName: tenant.StoreName,
            Status: tenant.Status.ToString(),
            CreatedAt: tenant.CreatedOnUtc,
            Domains: domains);
    }
}
