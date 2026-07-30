# Kromic Store Backend Implementation Guide

# Phase 02 -- 19 Indexes and Performance

**Version:** 1.0\
**Status:** Approved Foundation

------------------------------------------------------------------------

# Purpose

Define the indexing, query optimization, caching, and database
performance strategy for Kromic Store.

Goals:

-   Fast tenant-scoped queries
-   Predictable query plans
-   Scalable growth
-   Efficient reporting
-   Minimal database contention

------------------------------------------------------------------------

# Performance Principles

-   Tenant isolation first
-   Index intentionally
-   Avoid over-indexing
-   Optimize read-heavy operations
-   Measure before optimizing

------------------------------------------------------------------------

# Index Strategy

## Primary Keys

All tables use Guid (`uuid`) primary keys.

------------------------------------------------------------------------

## Composite Indexes

Tenant-owned tables should prefer composite indexes beginning with
`TenantId`.

Examples:

``` text
(TenantId, Email)
(TenantId, SKU)
(TenantId, Slug)
(TenantId, Status)
(TenantId, CreatedOnUtc)
```

------------------------------------------------------------------------

# Recommended Indexes

## Tenant

-   UX_Subdomain
-   UX_CustomDomain
-   IX_Status

## Users

-   UX_Tenant_Email
-   IX_LastLoginOnUtc

## Products

-   UX_Tenant_SKU
-   UX_Tenant_Slug
-   IX_Category
-   IX_Status
-   IX_IsFeatured

## Orders

-   UX_Tenant_OrderNumber
-   IX_Status
-   IX_Customer
-   IX_CreatedOnUtc

## Customers

-   UX_Tenant_Email
-   IX_Status

------------------------------------------------------------------------

# EF Core Query Guidelines

-   Use projections instead of loading full entities.
-   Prefer `AsNoTracking()` for read-only queries.
-   Avoid unnecessary `Include()` chains.
-   Use pagination for lists.
-   Never return unbounded collections.

------------------------------------------------------------------------

# Pagination

Default page size:

-   20

Maximum page size:

-   100

For very large datasets, prefer keyset pagination.

------------------------------------------------------------------------

# Search

Prepare for PostgreSQL Full Text Search.

Searchable:

-   Product Name
-   Description
-   SKU
-   Tags
-   Category

Future:

-   Elasticsearch

------------------------------------------------------------------------

# Caching

Initial strategy:

-   IMemoryCache

Cache:

-   Tenant resolution
-   Public themes
-   Store configuration
-   Reserved subdomains

Future:

-   Redis

------------------------------------------------------------------------

# Reporting

Recommended summary tables:

-   Daily sales
-   Monthly revenue
-   Top products
-   Top customers

Keep expensive analytics off transactional queries.

------------------------------------------------------------------------

# Database Maintenance

Schedule:

-   VACUUM
-   ANALYZE
-   REINDEX (when required)

Monitor index usage periodically.

------------------------------------------------------------------------

# Monitoring

Track:

-   Slow queries
-   Query duration
-   Index scans
-   Sequential scans
-   Lock contention

------------------------------------------------------------------------

# Performance Testing

Verify:

-   Product search
-   Category browsing
-   Checkout
-   Order history
-   Dashboard statistics
-   Tenant resolution

------------------------------------------------------------------------

# Anti-Patterns

Avoid:

-   SELECT \*
-   N+1 queries
-   Missing pagination
-   Long-running transactions
-   Loading unnecessary navigation properties

------------------------------------------------------------------------

# Future Enhancements

-   Redis cache
-   Read replicas
-   Materialized views
-   Query result caching
-   Background aggregation

------------------------------------------------------------------------

# Next Document

**20-EF-Core-Configuration.md**

Topics:

-   Entity configurations
-   Global query filters
-   Value converters
-   Concurrency
-   Naming conventions
-   DbContext configuration
-   SaveChanges pipeline
