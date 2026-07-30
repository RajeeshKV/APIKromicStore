# Kromic Store Backend Documentation

# Phase 06 -- 123 Database Standards

**Version:** 1.0\
**Status:** Approved Foundation

------------------------------------------------------------------------

# Purpose

This document defines the database standards for Kromic Store. These
standards ensure consistency, performance, maintainability, and data
integrity across all services and tenants.

------------------------------------------------------------------------

# Objectives

-   Standardize schema design
-   Ensure data integrity
-   Improve query performance
-   Simplify maintenance
-   Support scalable growth
-   Enable reliable migrations

------------------------------------------------------------------------

# Schema Organization

Organize database objects by logical domain.

Examples:

-   Identity
-   Catalog
-   Orders
-   CMS
-   Themes
-   Audit
-   BackgroundJobs

Avoid unnecessary cross-schema dependencies.

------------------------------------------------------------------------

# Naming Standards

Use consistent names:

-   Tables: `PascalCase`
-   Columns: `PascalCase`
-   Primary Keys: `Id`
-   Foreign Keys: `<Entity>NameId`
-   Indexes: `IX_Table_Column`
-   Unique Constraints: `UX_Table_Column`
-   Check Constraints: `CK_Table_Name`

Avoid abbreviations.

------------------------------------------------------------------------

# Primary Keys

-   Use UUIDs where distributed uniqueness is required.
-   Keep keys immutable.
-   Define clustered indexes appropriately.

------------------------------------------------------------------------

# Foreign Keys

-   Enforce referential integrity.
-   Index frequently queried foreign keys.
-   Avoid cascading deletes unless explicitly required.

------------------------------------------------------------------------

# Constraints

Use:

-   Primary Keys
-   Foreign Keys
-   Unique Constraints
-   Check Constraints
-   NOT NULL where applicable

Business rules should be enforced both in the application and database
where appropriate.

------------------------------------------------------------------------

# Indexing

Create indexes for:

-   Foreign keys
-   Search fields
-   TenantId
-   Frequently filtered columns
-   Frequently sorted columns

Review unused indexes periodically.

------------------------------------------------------------------------

# Multi-Tenant Design

Every tenant-owned table should include:

-   TenantId

Queries must always filter by TenantId.

Prevent cross-tenant data access.

------------------------------------------------------------------------

# Migrations

Use Entity Framework Core migrations.

Guidelines:

-   One logical change per migration
-   Review generated SQL
-   Test before production
-   Keep migrations idempotent where practical

------------------------------------------------------------------------

# Performance

Recommendations:

-   Optimize queries
-   Avoid SELECT \*
-   Paginate large result sets
-   Monitor execution plans
-   Archive historical data

------------------------------------------------------------------------

# Maintenance

Perform:

-   Index maintenance
-   Statistics updates
-   Backup verification
-   Integrity checks
-   Storage monitoring

Schedule maintenance during low-traffic periods.

------------------------------------------------------------------------

# Testing

Verify:

-   Constraints
-   Migrations
-   Index usage
-   Tenant isolation
-   Query performance
-   Restore compatibility

------------------------------------------------------------------------

# Best Practices

-   Design for consistency.
-   Index thoughtfully.
-   Keep schemas clean.
-   Review database performance regularly.
-   Treat migrations as production code.

------------------------------------------------------------------------

# Next Document

**124 -- Entity Framework Core Standards**

Topics:

-   DbContext
-   Entity configuration
-   Fluent API
-   Value converters
-   Query optimization
-   Tracking
-   Performance
