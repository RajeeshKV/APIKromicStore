using MediatR;

namespace KromicStore.Application.Features.Tenants.Commands.CreateTenant;

public sealed record CreateTenantCommand(
    string Name,
    string Subdomain,
    string? StoreName = null,
    Guid? OwnerUserId = null) : IRequest<CreateTenantResponse>;

public sealed record CreateTenantResponse(
    Guid TenantId,
    string Name,
    string Subdomain,
    string StoreName);
