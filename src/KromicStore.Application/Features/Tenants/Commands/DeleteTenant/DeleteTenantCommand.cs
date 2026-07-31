using MediatR;

namespace KromicStore.Application.Features.Tenants.Commands.DeleteTenant;

public sealed class DeleteTenantCommand : IRequest<Unit>
{
    public Guid TenantId { get; set; }
    public bool HardDelete { get; set; } = false; // If false, soft delete
}
