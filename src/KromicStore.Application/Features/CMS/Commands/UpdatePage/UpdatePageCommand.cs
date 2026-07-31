using MediatR;

namespace KromicStore.Application.Features.CMS.Commands.UpdatePage;

/// <summary>
/// Command to update an existing CMS page.
/// </summary>
public sealed record UpdatePageCommand(
    Guid PageId,
    Guid TenantId,
    string Title,
    string Slug,
    string Content,
    string? MetaDescription = null,
    string? MetaKeywords = null) : IRequest<UpdatePageResponse>;

/// <summary>
/// Response from UpdatePageCommand.
/// </summary>
public sealed record UpdatePageResponse(
    Guid PageId,
    string Title,
    string Slug,
    string Status,
    DateTime UpdatedAtUtc);
