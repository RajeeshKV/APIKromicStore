using MediatR;

namespace KromicStore.Application.Features.Tenants.Commands.AddCustomDomain;

public sealed record AddCustomDomainCommand(
    Guid TenantId,
    string CustomDomain,
    bool SetPrimary = false) : IRequest<AddCustomDomainResponse>;

public sealed record AddCustomDomainResponse(
    Guid TenantId,
    string CustomDomain,
    bool IsPrimary,
    bool IsVerified);
