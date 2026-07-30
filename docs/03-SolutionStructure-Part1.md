# Kromic Store Backend Implementation Guide

# Phase 01 - 03 Solution Structure

**Version:** 1.0\
**Part:** 1 of 3

------------------------------------------------------------------------

# 1. Purpose

This document defines the solution layout, project responsibilities,
dependency rules, and feature organization for Kromic Store.

The objectives are:

-   Consistent project organization
-   Clear separation of responsibilities
-   Clean Architecture compliance
-   Easy onboarding for new developers
-   Maintainability as the platform grows

------------------------------------------------------------------------

# 2. Repository Structure

``` text
KromicStore/
├── src/
├── tests/
├── docs/
├── build/
└── .github/
```

## Responsibilities

  Folder    Purpose
  --------- ----------------------------------------
  src       Production source code
  tests     Unit, Integration and API tests
  docs      Implementation guides and architecture
  build     Docker and deployment scripts
  .github   GitHub Actions workflows

------------------------------------------------------------------------

# 3. Source Projects

``` text
src/
├── KromicStore.Api
├── KromicStore.Application
├── KromicStore.Domain
├── KromicStore.Infrastructure
├── KromicStore.Contracts
└── KromicStore.SharedKernel
```

------------------------------------------------------------------------

# 4. Dependency Rules

``` text
Api
 │
 ▼
Application
 │
 ▼
Domain
 ▲
 │
Infrastructure

Contracts (shared with frontend)

SharedKernel (shared utilities)
```

## Rules

-   Api never contains business logic.
-   Domain references nothing.
-   Infrastructure implements Application interfaces.
-   Controllers communicate only through MediatR.
-   SharedKernel contains reusable cross-cutting code.

------------------------------------------------------------------------

# 5. Project Responsibilities

## KromicStore.Api

Contains:

-   Controllers
-   Middlewares
-   Swagger
-   API Versioning
-   Health Endpoints
-   Dependency Injection

Must never contain:

-   Business rules
-   SQL
-   EF queries

------------------------------------------------------------------------

## KromicStore.Application

Contains:

-   Commands
-   Queries
-   Handlers
-   Validators
-   DTOs
-   Behaviors
-   Interfaces

Responsible for orchestrating business use cases.

------------------------------------------------------------------------

## KromicStore.Domain

Contains:

-   Entities
-   Enums
-   Value Objects
-   Domain Events
-   Business Rules
-   Exceptions

Pure C# project.

------------------------------------------------------------------------

## KromicStore.Infrastructure

Contains:

-   EF Core
-   PostgreSQL
-   Cloudinary
-   Razorpay
-   Brevo
-   Authentication
-   Background Workers
-   Logging
-   Tenant Resolution

Implements Application interfaces.

------------------------------------------------------------------------

## KromicStore.Contracts

Contains:

-   Request Models
-   Response Models
-   Pagination Contracts
-   Shared API Contracts

------------------------------------------------------------------------

## KromicStore.SharedKernel

Contains:

-   Result Pattern
-   Base Entity
-   Audit Fields
-   Constants
-   Extensions
-   Guard Clauses
-   Pagination Helpers
-   Correlation ID Models

------------------------------------------------------------------------

# 6. Test Projects

``` text
tests/
├── KromicStore.UnitTests
├── KromicStore.IntegrationTests
└── KromicStore.ApiTests
```

Unit Tests

-   Business rules only

Integration Tests

-   PostgreSQL
-   EF Core
-   MediatR

API Tests

-   Full HTTP pipeline
-   Authentication
-   Authorization
-   Validation
-   Error contracts

------------------------------------------------------------------------

# 7. Documentation Structure

``` text
docs/
├── ImplementationGuide/
├── Architecture/
├── ADR/
├── API/
└── Deployment/
```

------------------------------------------------------------------------

# 8. Build Structure

``` text
build/
├── docker/
├── scripts/
├── render/
└── migrations/
```

------------------------------------------------------------------------

# 9. Feature Organization

Every feature is self-contained.

Example:

``` text
Products/
├── Commands/
├── Queries/
├── DTOs/
├── Validators/
├── Interfaces/
├── Events/
└── Mappings/
```

Example Authentication Feature:

``` text
Authentication/
├── Commands/
│   ├── Login/
│   ├── Register/
│   ├── RefreshToken/
│   └── VerifyEmail/
├── Queries/
├── DTOs/
├── Validators/
├── Interfaces/
└── Events/
```

------------------------------------------------------------------------

# 10. Feature Checklist

Every feature should include:

-   Commands
-   Queries
-   Validators
-   DTOs
-   Authorization
-   Unit Tests
-   Integration Tests
-   Swagger Examples
-   Documentation
-   Logging
-   Metrics

------------------------------------------------------------------------

# Next

Part 2 will cover:

-   Program.cs philosophy
-   Dependency Injection strategy
-   Extension methods
-   Configuration binding
-   Options Pattern
-   Environment loading
-   Service registration
-   Application startup pipeline
