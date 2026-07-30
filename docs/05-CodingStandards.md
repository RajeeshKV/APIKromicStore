# Kromic Store Backend Implementation Guide

# Phase 01 - 05 Coding Standards

**Version:** 1.0\
**Status:** Draft

------------------------------------------------------------------------

# 1. Purpose

This document defines the coding standards that every backend
contributor must follow.

Goals:

-   Consistent codebase
-   Predictable architecture
-   Easier reviews
-   Production-ready implementation

------------------------------------------------------------------------

# 2. General Principles

-   Follow SOLID principles.
-   Prefer composition over inheritance.
-   Keep methods focused on one responsibility.
-   Avoid premature optimization.
-   Prefer readability over clever code.
-   Every public API should be documented.

------------------------------------------------------------------------

# 3. C# Standards

-   Enable nullable reference types.
-   Treat warnings as errors in CI.
-   Use file-scoped namespaces.
-   Prefer `required` properties where appropriate.
-   Use `record` for immutable DTOs.
-   Use `sealed` for classes not intended for inheritance.

------------------------------------------------------------------------

# 4. Async Guidelines

-   Use async/await throughout.
-   Never block using `.Result` or `.Wait()`.
-   Accept `CancellationToken` in async handlers and services.
-   Pass the token to EF Core and external HTTP calls.

------------------------------------------------------------------------

# 5. Controller Standards

Controllers should:

-   Contain routing only.
-   Delegate work to MediatR.
-   Never access DbContext directly.
-   Return standardized API responses.
-   Be versioned (`/api/v1/...`).

------------------------------------------------------------------------

# 6. CQRS Standards

Every command folder contains:

``` text
CreateProduct/
├── CreateProductCommand.cs
├── CreateProductCommandHandler.cs
├── CreateProductCommandValidator.cs
└── CreateProductResponse.cs
```

Every query folder contains:

``` text
GetProductById/
├── GetProductByIdQuery.cs
├── GetProductByIdQueryHandler.cs
└── ProductDto.cs
```

------------------------------------------------------------------------

# 7. DTO Rules

-   Separate request and response DTOs.
-   Never expose EF entities.
-   Never expose internal identifiers unless required.
-   Prefer immutable records.

------------------------------------------------------------------------

# 8. Entity Rules

Every entity should include:

-   Id
-   CreatedOnUtc
-   ModifiedOnUtc
-   CreatedBy
-   ModifiedBy
-   TenantId (where applicable)
-   IsDeleted (for soft-delete entities)

Business rules belong in the domain model or application layer, not
controllers.

------------------------------------------------------------------------

# 9. EF Core Standards

-   One configuration class per entity.
-   No Fluent API inside DbContext.
-   Define indexes explicitly.
-   Configure delete behaviors intentionally.
-   Use global query filters for tenant isolation.

------------------------------------------------------------------------

# 10. Validation

Use FluentValidation.

Validation should cover:

-   Required fields
-   Length limits
-   Formats
-   Business rules where appropriate
-   Cross-field validation

------------------------------------------------------------------------

# 11. Error Handling

Use centralized exception middleware.

Never:

-   Catch and ignore exceptions.
-   Return stack traces.
-   Leak internal details.

All errors follow the standard API error contract.

------------------------------------------------------------------------

# 12. Logging

Log:

-   Startup
-   Shutdown
-   Requests
-   Exceptions
-   External service failures
-   Background worker execution

Always include:

-   CorrelationId
-   TenantId (if available)
-   UserId (if authenticated)

Never log passwords, secrets, or payment details.

------------------------------------------------------------------------

# 13. Configuration

-   Strongly typed Options classes.
-   Validate on startup.
-   Secrets only from environment variables.
-   No hardcoded URLs or credentials.

------------------------------------------------------------------------

# 14. Testing Standards

Every feature should have:

-   Unit tests
-   Integration tests
-   API tests (where applicable)

Test naming:

``` text
MethodName_ShouldExpectedBehavior_WhenCondition
```

Example:

``` text
CreateProduct_ShouldReturnConflict_WhenCodeAlreadyExists
```

------------------------------------------------------------------------

# 15. Pull Request Checklist

Before merging:

-   Code builds successfully.
-   Tests pass.
-   Documentation updated.
-   Logging included.
-   Validation implemented.
-   Authorization verified.
-   Swagger updated if API changed.

------------------------------------------------------------------------

# 16. Definition of Done

A feature is complete only when:

-   Business logic implemented.
-   Validation complete.
-   Logging added.
-   Tests passing.
-   Documentation updated.
-   Swagger reflects the API.
-   Security reviewed.

------------------------------------------------------------------------

# Next Document

**06-EnvironmentVariables.md**

Will define every environment variable used by Kromic Store, including
purpose, examples, required status, default values, Render
configuration, and security notes.
