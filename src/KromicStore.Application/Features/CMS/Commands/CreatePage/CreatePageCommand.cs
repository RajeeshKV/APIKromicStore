using MediatR;

namespace KromicStore.Application.Features.CMS.Commands.CreatePage;

/// <summary>
/// Command to create a new CMS page.
/// </summary>
public sealed record CreatePageCommand(
    Guid TenantId,
    string Title,
    string Slug,
    string Content,
    string? MetaDescription = null,
    string? MetaKeywords = null,
    bool Publish = false) : IRequest<CreatePageResponse>;

/// <summary>
/// Response from CreatePageCommand.
/// </summary>
public sealed record CreatePageResponse(
    Guid PageId,
    string Title,
    string Slug,
    string Status,
    DateTime CreatedAtUtc);
