using MediatR;
using KromicStore.Application.Common.Abstractions;
using KromicStore.Application.Features.CMS.Abstractions;
using KromicStore.Domain.CMS.Entities;
using Microsoft.Extensions.Logging;

namespace KromicStore.Application.Features.CMS.Commands.CreatePage;

/// <summary>
/// Handler for CreatePageCommand.
/// Creates a new CMS page and optionally publishes it.
/// </summary>
public sealed class CreatePageCommandHandler : IRequestHandler<CreatePageCommand, CreatePageResponse>
{
    private readonly ICMSPageRepository _repository;
    private readonly IApplicationDbContext _dbContext;
    private readonly ILogger<CreatePageCommandHandler> _logger;

    public CreatePageCommandHandler(
        ICMSPageRepository repository,
        IApplicationDbContext dbContext,
        ILogger<CreatePageCommandHandler> logger)
    {
        _repository = repository ?? throw new ArgumentNullException(nameof(repository));
        _dbContext = dbContext ?? throw new ArgumentNullException(nameof(dbContext));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    public async Task<CreatePageResponse> Handle(CreatePageCommand command, CancellationToken cancellationToken)
    {
        _logger.LogInformation("Creating new CMS page: Title={Title}, Slug={Slug}", command.Title, command.Slug);

        // Check if slug already exists
        var slugExists = await _repository.SlugExistsAsync(command.TenantId, command.Slug, cancellationToken: cancellationToken);
        if (slugExists)
        {
            _logger.LogWarning("Slug already exists: {Slug}", command.Slug);
            throw new InvalidOperationException($"A page with slug '{command.Slug}' already exists");
        }

        // Create the page
        var page = CMSPage.Create(
            command.TenantId,
            command.Title,
            command.Slug,
            command.Content,
            command.MetaDescription,
            command.MetaKeywords);

        // Publish if requested
        if (command.Publish)
        {
            page.Publish();
        }

        // Persist
        _repository.Add(page);
        await _dbContext.SaveChangesAsync(cancellationToken);

        _logger.LogInformation(
            "CMS page created successfully: PageId={PageId}, Title={Title}, Status={Status}",
            page.Id, page.Title, page.Status);

        return new CreatePageResponse(
            PageId: page.Id,
            Title: page.Title,
            Slug: page.Slug,
            Status: page.Status.ToString(),
            CreatedAtUtc: page.CreatedOnUtc);
    }
}
