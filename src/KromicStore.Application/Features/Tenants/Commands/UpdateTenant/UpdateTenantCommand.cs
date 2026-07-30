using MediatR;

namespace KromicStore.Application.Features.Tenants.Commands.UpdateTenant;

public sealed record UpdateTenantCommand(
    Guid TenantId,
    string? StoreName = null,
    Guid? OwnerUserId = null) : IRequest<UpdateTenantResponse>;

public sealed record UpdateTenantResponse(
    Guid TenantId,
    string StoreName,
    Guid? OwnerUserId);
