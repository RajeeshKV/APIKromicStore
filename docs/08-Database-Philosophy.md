# Kromic Store Backend Implementation Guide

# Phase 02 -- 08 Database Philosophy

**Version:** 1.0\
**Status:** Approved Foundation

------------------------------------------------------------------------

# Purpose

This document defines the database design principles that every entity,
migration, and EF Core configuration in Kromic Store must follow.

These rules are mandatory unless a documented architectural decision
overrides them.

------------------------------------------------------------------------

# Core Principles

-   Multi-tenant by design
-   Secure by default
-   EF Core first
-   PostgreSQL optimized
-   Cloud-ready
-   Backward-compatible migrations
-   Consistent naming
-   Minimal surprises

------------------------------------------------------------------------

# Primary Key Strategy

## Decision

Use **Guid** (`uuid` in PostgreSQL) for every primary key.

### Reasons

-   Native PostgreSQL support
-   Excellent .NET / EF Core support
-   Distributed ID generation
-   Prevents sequential ID enumeration
-   No additional libraries required

### Rule

Every entity shall expose:

``` csharp
public Guid Id { get; set; }
```

------------------------------------------------------------------------

# Tenant Isolation

Every tenant-owned entity contains:

``` text
TenantId (Guid)
```

Tenant resolution order:

1.  Custom Domain
2.  Subdomain
3.  TenantId from authenticated context
4.  Reject request if tenant cannot be resolved

TenantId must never be accepted directly from client requests.

------------------------------------------------------------------------

# Customer Ownership

Every customer belongs to exactly one tenant.

The same email address may exist under different tenants.

------------------------------------------------------------------------

# Soft Delete Strategy

## Decision

Every business table supports soft delete.

Required columns:

``` text
IsDeleted
DeletedOnUtc
DeletedBy
```

Physical deletion is reserved only for controlled archival or
maintenance operations.

------------------------------------------------------------------------

# Auditing

All business entities include:

``` text
CreatedOnUtc
CreatedBy
ModifiedOnUtc
ModifiedBy
```

Auditing applies to:

-   Tenants
-   Customers
-   Products
-   Categories
-   Themes
-   Orders
-   Addresses
-   Store configuration
-   Payments

------------------------------------------------------------------------

# UTC Policy

All timestamps are stored in UTC.

Display conversion is handled by the frontend.

------------------------------------------------------------------------

# Theme Strategy

Themes are immutable.

Editing a theme creates a new theme record.

No version history is maintained.

------------------------------------------------------------------------

# Subdomain Strategy

Each tenant owns one active subdomain.

Rules:

-   Global uniqueness
-   Case-insensitive comparison
-   Lowercase only
-   Availability checked before creation/update

Reserved names include:

-   admin
-   api
-   app
-   dashboard
-   docs
-   login
-   mail
-   support
-   www

------------------------------------------------------------------------

# Custom Domains

Support both:

-   tenant.kromic.in
-   customdomain.com

Requests are resolved to the owning tenant before any business logic
executes.

------------------------------------------------------------------------

# Cross-Tenant Protection

Protection exists at multiple layers:

1.  Tenant Resolution Middleware
2.  Tenant Context
3.  EF Core Global Query Filters
4.  Authorization checks
5.  Composite indexes using TenantId

Example query filter:

``` csharp
entity => entity.TenantId == CurrentTenantId
```

------------------------------------------------------------------------

# Naming Standards

Tables:

-   Singular entity names

Columns:

-   PascalCase

Foreign keys:

-   EntityNameId

Indexes:

-   IX_Table_Column

Unique constraints:

-   UX_Table_Column

------------------------------------------------------------------------

# Cascade Delete

Avoid cascade deletes for business entities.

Prefer:

-   Restrict
-   NoAction

Business logic controls deletion.

------------------------------------------------------------------------

# Index Philosophy

Index:

-   Foreign keys
-   Frequently filtered columns
-   Frequently sorted columns

Composite indexes should begin with TenantId when applicable.

Examples:

``` text
(TenantId, ProductCode)

(TenantId, CategoryId)

(TenantId, Email)
```

------------------------------------------------------------------------

# Migration Philosophy

-   Never edit an applied migration.
-   Create incremental migrations only.
-   Keep migrations small and descriptive.
-   Validate migrations before deployment.
-   Apply automatically during application startup.

------------------------------------------------------------------------

# Security

Never store:

-   Plaintext passwords
-   Razorpay secrets
-   API keys

Sensitive values stored in the database must be encrypted where
appropriate.

------------------------------------------------------------------------

# Future Compatibility

The schema should support:

-   Multiple storefronts
-   Marketplace features
-   Multi-language content
-   Inventory expansion
-   Analytics
-   Mobile applications

------------------------------------------------------------------------

# Architecture Decisions

  Topic                    Decision
  ------------------------ --------------------------------------------
  Primary Keys             Guid
  Soft Delete              Every business table
  Audit                    All business entities
  Customer Ownership       One tenant only
  Theme Versioning         New theme instead of revisions
  Tenant Resolution        Domain/Subdomain middleware
  Cross-Tenant Isolation   Middleware + Query Filters + Authorization
  Reserved Subdomains      Yes
  Physical Delete          Avoid in production

------------------------------------------------------------------------

# Next Document

**09-MultiTenant-Strategy.md**

Topics:

-   Tenant resolution middleware
-   Tenant context
-   Domain mapping
-   Custom domains
-   Global query filters
-   Authorization strategy
-   Request lifecycle
-   Performance considerations
