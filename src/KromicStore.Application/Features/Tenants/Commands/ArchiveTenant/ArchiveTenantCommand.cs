using MediatR;

namespace KromicStore.Application.Features.Tenants.Commands.ArchiveTenant;

public sealed record ArchiveTenantCommand(Guid TenantId) : IRequest<Unit>;
