# Kromic Store Backend Implementation Guide

# Phase 02 -- 09 Multi-Tenant Strategy

**Version:** 1.0\
**Status:** Approved Foundation

------------------------------------------------------------------------

# Purpose

This document defines how tenant isolation, tenant resolution, and
request routing work throughout Kromic Store.

Every request must resolve a tenant before business logic executes.

------------------------------------------------------------------------

# Goals

-   Complete tenant isolation
-   High performance
-   Secure request processing
-   Flexible domain support
-   Simple developer experience

------------------------------------------------------------------------

# Tenant Identification

A tenant can be resolved using:

1.  Custom Domain
2.  Kromic Subdomain
3.  TenantId from authenticated JWT (internal APIs only)

Priority is always:

``` text
Custom Domain
      ↓
Subdomain
      ↓
Authenticated TenantId
      ↓
Tenant Not Found
```

------------------------------------------------------------------------

# Supported URLs

Examples:

``` text
flowers.kromic.in
electronics.kromic.in
mybrand.com
shop.mybrand.com
```

------------------------------------------------------------------------

# Request Lifecycle

``` text
Incoming Request
      │
Extract Host Header
      │
Normalize Host
      │
Lookup Tenant
      │
Validate Active Status
      │
Store TenantContext
      │
Continue Middleware Pipeline
      │
Controller
      │
CQRS Handler
      │
EF Core Query Filter
```

------------------------------------------------------------------------

# Tenant Middleware

Responsibilities:

-   Read Host header
-   Remove ports
-   Normalize lowercase
-   Resolve tenant
-   Reject inactive tenants
-   Populate TenantContext
-   Stop request when tenant is invalid

Return:

-   404 - Tenant not found
-   403 - Tenant disabled

------------------------------------------------------------------------

# TenantContext

Available through dependency injection.

Properties:

``` text
TenantId
TenantName
Subdomain
CustomDomain
StoreName
IsActive
```

Never mutate TenantContext during a request.

------------------------------------------------------------------------

# Domain Resolution Rules

## Subdomains

Requirements:

-   Globally unique
-   Lowercase only
-   Case-insensitive
-   Reserved names blocked

## Custom Domains

Requirements:

-   One tenant owns one custom domain
-   Domain ownership must be unique
-   HTTPS required in production

------------------------------------------------------------------------

# Reserved Subdomains

Examples:

-   admin
-   api
-   app
-   dashboard
-   docs
-   login
-   mail
-   support
-   www

Maintain the list in configuration.

------------------------------------------------------------------------

# EF Core Query Filters

Every tenant-owned entity uses a global query filter.

Example:

``` csharp
builder.HasQueryFilter(e =>
    !e.IsDeleted &&
    e.TenantId == tenantContext.TenantId);
```

This provides automatic tenant isolation.

------------------------------------------------------------------------

# Authorization

Tenant resolution does not replace authorization.

Every protected endpoint must also verify:

-   User belongs to tenant
-   Required role exists
-   Resource belongs to tenant

------------------------------------------------------------------------

# Performance

Recommendations:

-   Cache tenant lookups in memory
-   Cache by host name
-   Refresh cache when tenant settings change
-   Index Subdomain and CustomDomain

Suggested indexes:

``` text
UX_Tenant_Subdomain
UX_Tenant_CustomDomain
```

------------------------------------------------------------------------

# Failure Scenarios

  Scenario              Result
  --------------------- --------------------
  Unknown subdomain     404
  Disabled tenant       403
  Duplicate domain      Validation failure
  Reserved subdomain    Validation failure
  Missing Host header   400

------------------------------------------------------------------------

# Security

-   Never trust TenantId from public requests
-   Resolve tenant from host whenever possible
-   Prevent cross-tenant data access using middleware, authorization,
    and EF Core query filters
-   Log TenantId with every authenticated request

------------------------------------------------------------------------

# Future Enhancements

-   Redis tenant cache
-   Multi-region routing
-   CDN-aware tenant resolution
-   Wildcard SSL automation

------------------------------------------------------------------------

# Architecture Decisions

  Topic                Decision
  -------------------- -------------------------
  Tenant Resolution    Middleware
  Primary Resolution   Host Header
  Query Isolation      EF Global Query Filters
  Custom Domains       Supported
  Subdomains           Globally Unique
  Reserved Domains     Configurable
  Tenant Cache         In-memory initially

------------------------------------------------------------------------

# Next Document

**10-BaseEntities-And-Auditing.md**

Topics:

-   BaseEntity
-   TenantEntity
-   AuditableEntity
-   Soft delete model
-   Concurrency strategy
-   Shared interfaces
-   EF Core base configurations
