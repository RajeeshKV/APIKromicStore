using MediatR;

namespace KromicStore.Application.Features.Tenants.Commands.VerifyCustomDomain;

public sealed record VerifyCustomDomainCommand(
    Guid TenantId,
    string CustomDomain) : IRequest<VerifyCustomDomainResponse>;

public sealed record VerifyCustomDomainResponse(
    Guid TenantId,
    string CustomDomain,
    bool IsVerified);
