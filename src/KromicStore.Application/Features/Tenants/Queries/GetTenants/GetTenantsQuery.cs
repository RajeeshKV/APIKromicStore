using MediatR;

namespace KromicStore.Application.Features.Tenants.Queries.GetTenants;

/// <summary>
/// Query to retrieve list of all tenants with optional filtering and pagination.
/// Supports search by name and filtering by status.
/// </summary>
public sealed class GetTenantsQuery : IRequest<TenantsPagedResponse>
{
    public int Skip { get; set; } = 0;
    public int Take { get; set; } = 20;
    public string? Status { get; set; }
    public string? Search { get; set; }
}

public sealed class TenantSummaryDto
{
    public Guid Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string StoreName { get; set; } = string.Empty;
    public string Slug { get; set; } = string.Empty;
    public string Status { get; set; } = string.Empty;
    public DateTime CreatedOnUtc { get; set; }
    public int OrderCount { get; set; }
    public decimal TotalRevenue { get; set; }
}

public sealed class TenantsPagedResponse
{
    public List<TenantSummaryDto> Tenants { get; set; } = new();
    public int TotalCount { get; set; }
    public int Skip { get; set; }
    public int Take { get; set; }
}
