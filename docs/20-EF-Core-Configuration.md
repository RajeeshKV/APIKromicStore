# Kromic Store Backend Implementation Guide

# Phase 02 -- 20 EF Core Configuration

**Version:** 1.0\
**Status:** Approved Foundation

------------------------------------------------------------------------

# Purpose

This document defines the Entity Framework Core configuration standards
for Kromic Store.

Goals:

-   Consistent entity configuration
-   Centralized conventions
-   Automatic tenant isolation
-   Automatic auditing
-   Predictable migrations
-   High-performance data access

------------------------------------------------------------------------

# DbContext Responsibilities

The DbContext should:

-   Register all DbSets
-   Apply entity configurations from assemblies
-   Configure global query filters
-   Handle audit field population
-   Execute transactions
-   Never contain business logic

------------------------------------------------------------------------

# Entity Configuration

Every entity must have its own configuration class.

Example:

``` text
Entities/
    Product.cs

Configurations/
    ProductConfiguration.cs
```

Never configure entities inside DbContext.

------------------------------------------------------------------------

# Global Query Filters

Apply automatically for:

-   Soft delete
-   Tenant isolation

Example behavior:

-   Exclude `IsDeleted = true`
-   Restrict records to the current TenantId

------------------------------------------------------------------------

# SaveChanges Pipeline

Before saving:

-   Populate CreatedOnUtc
-   Populate ModifiedOnUtc
-   Populate CreatedBy
-   Populate ModifiedBy
-   Set TenantId for tenant-owned entities
-   Convert deletes into soft deletes

------------------------------------------------------------------------

# Audit Rules

On Insert:

-   CreatedOnUtc
-   CreatedBy

On Update:

-   ModifiedOnUtc
-   ModifiedBy

Background workers should use a system identity.

------------------------------------------------------------------------

# Soft Delete

Delete operations should:

-   Set IsDeleted = true
-   Populate DeletedOnUtc
-   Populate DeletedBy

Avoid physical deletes in application code.

------------------------------------------------------------------------

# Concurrency

Use optimistic concurrency.

Recommended:

-   PostgreSQL `xmin` concurrency token

Concurrency exceptions should be translated into user-friendly API
responses.

------------------------------------------------------------------------

# Transactions

Use explicit transactions for operations involving:

-   Orders
-   Payments
-   Inventory
-   Outbox events

Ensure atomic commits.

------------------------------------------------------------------------

# Value Converters

Recommended converters:

-   UTC DateTime
-   Enum to string (where readability is preferred)
-   Strongly typed identifiers (future)

------------------------------------------------------------------------

# Naming Conventions

Tables:

-   Singular names

Columns:

-   PascalCase

Foreign Keys:

-   EntityNameId

Indexes:

-   IX_Table_Column

Unique Constraints:

-   UX_Table_Column

------------------------------------------------------------------------

# Performance

Recommendations:

-   Use `AsNoTracking()` for queries
-   Project to DTOs
-   Avoid unnecessary Includes
-   Batch writes when possible

------------------------------------------------------------------------

# Migrations

Rules:

-   One migration per logical change
-   Never edit applied migrations
-   Descriptive migration names
-   Apply automatically during startup

------------------------------------------------------------------------

# Testing

Verify:

-   Global query filters
-   Audit population
-   Soft delete behavior
-   Concurrency handling
-   Migration execution

------------------------------------------------------------------------

# Architecture Decisions

  Topic                 Decision
  --------------------- ---------------------------------
  Configuration Style   Fluent API
  Query Filters         Global
  Audit                 Automatic
  Tenant Isolation      Global Filters
  Soft Delete           SaveChanges Pipeline
  Concurrency           Optimistic (`xmin`)
  Transactions          Explicit for critical workflows

------------------------------------------------------------------------

# Next Document

**21-Migrations-And-Seeding.md**

Topics:

-   Migration strategy
-   Seed data
-   Super User bootstrap
-   Reserved subdomains
-   Default roles
-   System themes
-   Initial configuration
