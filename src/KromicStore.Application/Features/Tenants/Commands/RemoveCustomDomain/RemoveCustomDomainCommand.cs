using MediatR;

namespace KromicStore.Application.Features.Tenants.Commands.RemoveCustomDomain;

public sealed record RemoveCustomDomainCommand(
    Guid TenantId,
    string CustomDomain) : IRequest<Unit>;
