using MediatR;

namespace KromicStore.Application.Features.CMS.Commands.DeletePage;

/// <summary>
/// Command to delete (soft-delete) a CMS page.
/// </summary>
public sealed record DeletePageCommand(
    Guid PageId,
    Guid TenantId) : IRequest<DeletePageResponse>;

/// <summary>
/// Response from DeletePageCommand.
/// </summary>
public sealed record DeletePageResponse(
    Guid PageId,
    bool Success,
    string Message);
