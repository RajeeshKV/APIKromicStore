# Kromic Store Backend Implementation Guide

# Phase 02 -- 21 Migrations and Seeding

**Version:** 1.0\
**Status:** Approved Foundation

------------------------------------------------------------------------

# Purpose

Define the strategy for Entity Framework Core migrations, database
initialization, and seed data management for Kromic Store.

Goals:

-   Repeatable deployments
-   Safe schema evolution
-   Idempotent startup
-   Predictable environments
-   Minimal manual intervention

------------------------------------------------------------------------

# Migration Philosophy

Rules:

-   One migration per logical change
-   Never modify an applied migration
-   Keep migrations small and descriptive
-   Review generated SQL before production
-   Commit migrations with the related code

Examples:

-   AddProductVariants
-   IntroduceThemeAssignments
-   AddCustomerPreferences

------------------------------------------------------------------------

# Startup Flow

``` text
Application Starts
      │
Load Configuration
      │
Validate Settings
      │
Open Database Connection
      │
Apply Pending Migrations
      │
Run Seeders
      │
Start Hosted Services
      │
Application Ready
```

------------------------------------------------------------------------

# Automatic Migration

The API should:

-   Detect pending migrations
-   Apply them automatically
-   Stop startup if migration fails

Do not ignore migration failures.

------------------------------------------------------------------------

# Seed Categories

## System Seed Data

-   Default roles
-   Super User
-   Reserved subdomains
-   System themes
-   Default permissions

------------------------------------------------------------------------

## Reference Data

Examples:

-   Countries
-   Time zones
-   Languages
-   Currencies (if required)

Reference data should change rarely.

------------------------------------------------------------------------

## Development Seed Data

Development only:

-   Demo tenant
-   Demo products
-   Demo customers
-   Sample orders

Never seed demo data in production.

------------------------------------------------------------------------

# Super User Bootstrap

Create the initial Super User only if one does not already exist.

Configuration should come from environment variables:

-   Email
-   Password
-   Name

Passwords must be hashed using ASP.NET Core PasswordHasher.

------------------------------------------------------------------------

# Reserved Subdomains

Seed once.

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

Maintain through migrations or dedicated seeders.

------------------------------------------------------------------------

# Theme Seeding

Seed built-in themes.

Properties:

-   Read-only
-   Public
-   Available to all tenants

Tenants clone system themes before customization.

------------------------------------------------------------------------

# Seeder Design

Each seeder should have one responsibility.

Suggested structure:

``` text
Seeders/
├── RoleSeeder
├── SuperUserSeeder
├── ThemeSeeder
├── ReservedSubdomainSeeder
└── ReferenceDataSeeder
```

Seeders should be idempotent.

------------------------------------------------------------------------

# Environment Rules

Development:

-   Run demo seeders

Production:

-   Run only system seeders

Testing:

-   Seed only required test data

------------------------------------------------------------------------

# Rollback Strategy

If deployment fails:

1.  Investigate migration logs
2.  Restore from backup if required
3.  Deploy corrective migration
4.  Avoid editing historical migrations

------------------------------------------------------------------------

# CI/CD

Pipeline order:

1.  Restore
2.  Build
3.  Test
4.  Publish
5.  Build Docker Image
6.  Deploy
7.  Run Migrations
8.  Health Check

------------------------------------------------------------------------

# Testing

Verify:

-   Fresh database creation
-   Upgrade from previous version
-   Seeder idempotency
-   Super User creation
-   Reserved subdomains
-   Theme seeding
-   Migration rollback plan

------------------------------------------------------------------------

# Best Practices

-   Backup production before major schema changes
-   Avoid destructive migrations
-   Use transactions where supported
-   Review migration SQL for large datasets

------------------------------------------------------------------------

# Architecture Decisions

  Topic                 Decision
  --------------------- -------------------------------
  Migration Execution   Automatic on startup
  Seeder Design         One responsibility per seeder
  Super User            Created once
  Demo Data             Development only
  System Themes         Seeded
  Reserved Domains      Seeded

------------------------------------------------------------------------

# Next Document

**22-Database-ER-Diagrams.md**

Topics:

-   Complete entity relationship diagrams
-   Aggregate boundaries
-   Foreign key map
-   Navigation relationships
-   Tenant ownership matrix
-   Database dependency graph
