# Kromic Store Backend Documentation

# Phase 06 -- 84 Solution Structure

**Version:** 1.0\
**Status:** Approved Foundation

------------------------------------------------------------------------

# Purpose

This document defines the recommended solution and repository structure
for the Kromic Store backend. A consistent structure improves
maintainability, discoverability, onboarding, testing, and long-term
scalability.

------------------------------------------------------------------------

# Objectives

-   Organize code by responsibility
-   Enforce architectural boundaries
-   Promote modular development
-   Simplify testing and deployment
-   Support future platform growth

------------------------------------------------------------------------

# Repository Layout

    KromicStore/
    │
    ├── src/
    ├── tests/
    ├── docs/
    ├── scripts/
    ├── docker/
    ├── .github/
    ├── tools/
    └── README.md

------------------------------------------------------------------------

# Source Structure

    src/
    │
    ├── KromicStore.API
    ├── KromicStore.Application
    ├── KromicStore.Domain
    ├── KromicStore.Infrastructure
    ├── KromicStore.Shared
    └── KromicStore.Contracts

------------------------------------------------------------------------

# Project Responsibilities

## KromicStore.API

Contains:

-   Controllers / Minimal APIs
-   Middleware
-   Authentication
-   Dependency Injection
-   OpenAPI
-   API Versioning
-   Request/Response models

------------------------------------------------------------------------

## KromicStore.Application

Contains:

-   Commands
-   Queries
-   Handlers
-   DTOs
-   Interfaces
-   Validators
-   Mapping
-   Pipeline Behaviors

Must not depend on API.

------------------------------------------------------------------------

## KromicStore.Domain

Contains:

-   Entities
-   Aggregates
-   Value Objects
-   Enumerations
-   Domain Events
-   Business Rules
-   Specifications

Must have no infrastructure dependencies.

------------------------------------------------------------------------

## KromicStore.Infrastructure

Contains:

-   EF Core
-   Repositories
-   Database Context
-   External Integrations
-   Cloudinary
-   Brevo
-   Payment Providers
-   Background Jobs
-   Persistence

Implements interfaces defined in the Application layer.

------------------------------------------------------------------------

## KromicStore.Shared

Contains reusable utilities:

-   Result types
-   Exceptions
-   Constants
-   Extensions
-   Common helpers
-   Base abstractions

Avoid placing business logic here.

------------------------------------------------------------------------

## KromicStore.Contracts

Contains shared contracts such as:

-   API request models
-   API response models
-   Events
-   Integration contracts

Useful for SDKs or external integrations.

------------------------------------------------------------------------

# Tests

    tests/

    Application.Tests
    Domain.Tests
    Infrastructure.Tests
    API.Tests
    Integration.Tests
    Performance.Tests

Each project should mirror the structure of the source project it
validates.

------------------------------------------------------------------------

# Documentation

    docs/

    architecture/
    api/
    database/
    deployment/
    operations/
    runbooks/

Maintain version-controlled documentation alongside the codebase.

------------------------------------------------------------------------

# Scripts

Store reusable scripts for:

-   Local setup
-   Database migration
-   Seeding
-   Docker
-   Deployment
-   Backup
-   Maintenance

------------------------------------------------------------------------

# Configuration

Organize configuration using:

-   appsettings.json
-   appsettings.Development.json
-   appsettings.Production.json
-   Environment variables
-   Options pattern

Avoid hardcoded configuration values.

------------------------------------------------------------------------

# Naming Conventions

Projects:

-   KromicStore.API
-   KromicStore.Application
-   KromicStore.Domain
-   KromicStore.Infrastructure

Classes:

-   PascalCase

Methods:

-   PascalCase

Private fields:

-   \_camelCase

Interfaces:

-   Prefix with I

Database:

-   Singular table names (or follow one consistent convention)
-   Snake_case if using PostgreSQL naming conventions

------------------------------------------------------------------------

# Dependency Rules

Allowed dependency flow:

API ↓ Application ↓ Domain

Infrastructure depends on:

-   Application
-   Domain

Domain must not reference any other project.

------------------------------------------------------------------------

# Shared Kernel

Only place code in the shared kernel when it is:

-   Generic
-   Stable
-   Widely reused
-   Independent of business rules

Avoid creating a "miscellaneous" project.

------------------------------------------------------------------------

# Folder Organization

Within each project, organize by feature first, then type where
practical.

Example:

    Products/
        Commands/
        Queries/
        Validators/
        DTOs/

Prefer feature-based organization over large global folders.

------------------------------------------------------------------------

# Best Practices

-   Keep project references minimal.
-   Enforce dependency direction.
-   Organize by feature.
-   Avoid circular dependencies.
-   Keep infrastructure replaceable.

------------------------------------------------------------------------

# Next Document

**85 -- Clean Architecture**

Topics:

-   Architectural principles
-   Layer responsibilities
-   Dependency inversion
-   Use case boundaries
-   Domain purity
-   Implementation guidelines
