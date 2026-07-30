# Kromic Store Backend Implementation Guide

# Phase 01 - 03 Solution Structure

**Version:** 1.0\
**Part:** 3 of 3

------------------------------------------------------------------------

# 23. Naming Conventions

## Projects

``` text
KromicStore.Api
KromicStore.Application
KromicStore.Domain
KromicStore.Infrastructure
```

## Commands

``` text
CreateProductCommand
UpdateProductCommand
DeleteProductCommand
```

## Queries

``` text
GetProductByIdQuery
SearchProductsQuery
GetDashboardSummaryQuery
```

## Handlers

``` text
CreateProductCommandHandler
GetProductByIdQueryHandler
```

## Validators

``` text
CreateProductCommandValidator
LoginCommandValidator
```

## DTOs

``` text
ProductDto
OrderSummaryDto
TenantProfileDto
```

------------------------------------------------------------------------

# 24. Namespace Standards

Namespaces mirror folders.

Example:

``` text
KromicStore.Application.Features.Products.Commands.CreateProduct
```

Avoid generic namespaces such as `Helpers`, `Utils`, or `Misc`.

------------------------------------------------------------------------

# 25. Feature Template

Each feature should follow the same layout.

``` text
Products/
├── Commands/
│   ├── CreateProduct/
│   │   ├── CreateProductCommand.cs
│   │   ├── CreateProductCommandHandler.cs
│   │   ├── CreateProductCommandValidator.cs
│   │   └── CreateProductResponse.cs
│   ├── UpdateProduct/
│   └── DeleteProduct/
├── Queries/
│   ├── GetProductById/
│   ├── SearchProducts/
│   └── GetProducts/
├── DTOs/
├── Events/
├── Interfaces/
└── Mappings/
```

------------------------------------------------------------------------

# 26. Entity Organization

Infrastructure

``` text
Persistence/
├── Configurations/
├── Migrations/
└── ApplicationDbContext.cs
```

Domain

``` text
Entities/
├── Tenant.cs
├── Product.cs
├── Category.cs
├── Order.cs
└── Customer.cs
```

Every entity has a dedicated EF configuration class.

------------------------------------------------------------------------

# 27. API Controller Standards

Controllers should:

-   Be versioned.
-   Contain only endpoint definitions.
-   Delegate work to MediatR.
-   Return consistent API responses.

Example:

``` csharp
[HttpPost]
public async Task<IActionResult> Create(CreateProductCommand command)
{
    var result = await _mediator.Send(command);
    return CreatedAtAction(nameof(GetById), new { id = result.Id }, result);
}
```

------------------------------------------------------------------------

# 28. DTO Rules

DTOs must:

-   Be immutable where practical.
-   Never expose EF entities.
-   Never include internal fields.
-   Represent API contracts only.

Separate request and response models.

------------------------------------------------------------------------

# 29. EF Configuration Standards

Each entity has:

``` text
ProductConfiguration.cs
CategoryConfiguration.cs
OrderConfiguration.cs
```

Configuration includes:

-   Keys
-   Indexes
-   Relationships
-   Constraints
-   Global filters

No fluent configuration inside DbContext.

------------------------------------------------------------------------

# 30. Middleware Organization

``` text
Middlewares/
├── CorrelationIdMiddleware.cs
├── ExceptionMiddleware.cs
├── TenantResolutionMiddleware.cs
├── RequestLoggingMiddleware.cs
└── SecurityHeadersMiddleware.cs
```

Each middleware has one responsibility.

------------------------------------------------------------------------

# 31. Background Workers

``` text
BackgroundWorkers/
├── EmailOutboxWorker.cs
├── RefundWorker.cs
├── StatisticsWorker.cs
└── CleanupWorker.cs
```

Workers:

-   Idempotent
-   Logged
-   CancellationToken aware
-   Retry transient failures

------------------------------------------------------------------------

# 32. Testing Standards

Every feature includes:

-   Unit tests
-   Integration tests
-   API tests

Naming:

``` text
CreateProductCommandTests
CreateProductIntegrationTests
ProductControllerTests
```

------------------------------------------------------------------------

# 33. Coding Standards

-   Async all the way.
-   UTC timestamps only.
-   No magic strings.
-   Prefer constructor injection.
-   Use CancellationToken.
-   Validate all commands.
-   Never swallow exceptions.
-   Keep methods focused.
-   Prefer composition over inheritance.

------------------------------------------------------------------------

# 34. Pull Request Checklist

Every PR should verify:

-   Documentation updated.
-   Tests added or updated.
-   Logging included.
-   Validation implemented.
-   Authorization checked.
-   Swagger updated.
-   No breaking changes without review.

------------------------------------------------------------------------

# 35. Summary

This document defines the structural standards for the entire Kromic
Store solution.

Following these conventions ensures:

-   Consistent codebase
-   Easier maintenance
-   Predictable project organization
-   Scalable feature development

------------------------------------------------------------------------

# Next Document

**04-TechnologyStack.md**

This document will justify every library, NuGet package, frontend
dependency, version strategy, and integration used across the platform.
