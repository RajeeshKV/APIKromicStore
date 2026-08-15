using KromicStore.Application.Features.Tenants.Abstractions;
using MediatR;

namespace KromicStore.Application.Features.Authentication.Queries.CheckSubdomain;

/// <summary>
/// Checks subdomain availability by running three checks in order:
///   1. Format validation (already done in controller but belt-and-suspenders)
///   2. Reserved subdomain list (store, admin, api, etc.)
///   3. Database uniqueness check
/// Returns immediately on first failure so the UI gets the most relevant message.
/// </summary>
public sealed class CheckSubdomainQueryHandler : IRequestHandler<CheckSubdomainQuery, CheckSubdomainResult>
{
    private readonly ITenantRepository        _tenantRepository;
    private readonly IReservedSubdomainService _reservedSubdomainService;

    public CheckSubdomainQueryHandler(
        ITenantRepository        tenantRepository,
        IReservedSubdomainService reservedSubdomainService)
    {
        _tenantRepository         = tenantRepository;
        _reservedSubdomainService = reservedSubdomainService;
    }

    public async Task<CheckSubdomainResult> Handle(CheckSubdomainQuery request, CancellationToken cancellationToken)
    {
        var subdomain = request.Subdomain.Trim().ToLowerInvariant();

        // 1. Reserved check
        if (_reservedSubdomainService.IsReserved(subdomain))
            return new CheckSubdomainResult(false, subdomain, "This subdomain is reserved by the platform.");

        // 2. DB uniqueness check
        var taken = await _tenantRepository.SubdomainExistsAsync(subdomain, cancellationToken: cancellationToken);
        if (taken)
            return new CheckSubdomainResult(false, subdomain, "This subdomain is already taken.");

        return new CheckSubdomainResult(true, subdomain);
    }
}
