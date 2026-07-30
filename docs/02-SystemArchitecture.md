# Kromic Store Backend Implementation Guide

# Phase 01 - 02 System Architecture

**Version:** 1.0

------------------------------------------------------------------------

# 1. Architecture Overview

Kromic Store is designed as a production-ready, multi-tenant SaaS
platform using Clean Architecture and CQRS. The API is stateless,
horizontally scalable, and suitable for deployment on Render Free Plan.

Core principles:

-   UI-driven API design
-   Feature-based organization
-   Tenant isolation
-   Environment-variable configuration
-   Small controllers
-   Background processing for long-running tasks

------------------------------------------------------------------------

# 2. High-Level Architecture

``` text
React (Vite)
        │
        ▼
 ASP.NET Core API
        │
──────────────────────────────────────
Correlation ID Middleware
Exception Middleware
Security Headers
Tenant Resolution
Authentication
Authorization
Rate Limiting
Controllers
──────────────────────────────────────
        │
        ▼
Application (CQRS)
        │
        ▼
Domain
        │
        ▼
Infrastructure
        │
 ├── PostgreSQL (EF Core)
 ├── Cloudinary
 ├── Razorpay
 ├── Brevo
 └── Background Workers
```

------------------------------------------------------------------------

# 3. Solution Layers

## Presentation

Responsibilities

-   Controllers
-   Swagger
-   Middlewares
-   Filters
-   Dependency registration

Contains **no business logic**.

------------------------------------------------------------------------

## Application

Contains:

-   Commands
-   Queries
-   Handlers
-   DTOs
-   Validators
-   Interfaces
-   Behaviors

Responsible for orchestration only.

------------------------------------------------------------------------

## Domain

Contains:

-   Entities
-   Enums
-   Value Objects
-   Domain Events
-   Business Rules

No dependency on EF Core or ASP.NET.

------------------------------------------------------------------------

## Infrastructure

Contains:

-   EF Core
-   Repository implementations (only where justified)
-   External services
-   Cloudinary
-   Razorpay
-   Brevo
-   Authentication
-   Persistence
-   Background workers

------------------------------------------------------------------------

# 4. Request Lifecycle

``` text
HTTP Request
      │
Correlation Id
      │
Request Logging
      │
Global Exception Handler
      │
Security Headers
      │
Tenant Resolution
      │
JWT Authentication
      │
Authorization
      │
Controller
      │
Mediator
      │
Command / Query
      │
Handler
      │
DbContext / External Services
      │
HTTP Response
```

------------------------------------------------------------------------

# 5. Middleware Order

1.  Correlation ID
2.  Serilog Request Logging
3.  Global Exception Handler
4.  Security Headers
5.  Tenant Resolution
6.  Authentication
7.  Authorization
8.  Rate Limiting
9.  Endpoint Execution

Changing this order requires architectural review.

------------------------------------------------------------------------

# 6. CQRS Flow

Commands

-   Create
-   Update
-   Delete

Queries

-   Read
-   Search
-   Dashboard
-   Statistics

Controllers never access DbContext directly.

------------------------------------------------------------------------

# 7. MediatR Pipeline Behaviors

Execution order:

``` text
Validation
    │
Logging
    │
Performance Timing
    │
Handler
```

Future behaviors:

-   Caching
-   Authorization
-   Metrics

------------------------------------------------------------------------

# 8. Tenant Resolution

Resolution priority:

1.  Custom domain
2.  Subdomain (\*.kromic.in)
3.  Development override header

Resolved TenantId is stored in a scoped TenantContext service.

Global query filters automatically apply TenantId.

------------------------------------------------------------------------

# 9. Authentication Architecture

Supported:

-   Email + Password
-   Google OAuth
-   JWT
-   Refresh Tokens
-   Token Versioning
-   Email Verification

Refresh tokens are rotated after every refresh request.

------------------------------------------------------------------------

# 10. External Service Architecture

Cloudinary

-   Upload images
-   Delete images
-   Generate optimized URLs

Razorpay

-   Create payment order
-   Verify payment
-   Refund payment

Brevo

-   Transactional emails
-   Templates
-   Notifications

Every provider is accessed through an interface and proxy service with
retry logic.

------------------------------------------------------------------------

# 11. Retry Strategy

External calls use Polly.

Retry policy:

-   Exponential backoff
-   Retry transient failures only
-   Log every retry
-   Correlation ID included

No retries for validation failures.

------------------------------------------------------------------------

# 12. Background Workers

Planned workers:

-   Email Outbox
-   Refund Processor
-   Statistics Aggregator
-   Cleanup Refresh Tokens
-   Cleanup Expired Carts

Workers never expose HTTP endpoints.

------------------------------------------------------------------------

# 13. Outbox Pattern

Business transaction:

1.  Save business entity
2.  Save outbox event
3.  Commit transaction

Worker:

1.  Read outbox
2.  Execute action
3.  Mark completed
4.  Retry on transient failures

Guarantees email consistency.

------------------------------------------------------------------------

# 14. Correlation ID

Generated if absent.

Returned in every response header.

Included in:

-   Logs
-   Errors
-   External service calls
-   Background workers

Allows end-to-end request tracing.

------------------------------------------------------------------------

# 15. Error Contract

Every API returns the same structure.

``` json
{
  "success": false,
  "correlationId": "...",
  "statusCode": 400,
  "errorCode": "VALIDATION_ERROR",
  "message": "Validation failed.",
  "errors": [
    {
      "field": "email",
      "message": "Email is required."
    }
  ]
}
```

No stack traces are exposed.

------------------------------------------------------------------------

# 16. Deployment Architecture

``` text
Vercel
    │
React Storefront
    │
Render
ASP.NET Core API
    │
Supabase PostgreSQL
    │
Cloudinary
Brevo
Razorpay
```

Docker startup:

1.  Load environment variables
2.  Configure logging
3.  Verify database
4.  Apply EF migrations
5.  Seed Super User (if required)
6.  Start API
7.  Expose health endpoint

------------------------------------------------------------------------

# 17. Design Patterns

Used intentionally:

-   Clean Architecture
-   CQRS
-   Mediator
-   Result Pattern
-   Options Pattern
-   Strategy
-   Factory
-   Proxy
-   Retry
-   Outbox
-   Domain Events

Avoid unnecessary abstractions.

------------------------------------------------------------------------

# 18. Architectural Rules

-   No business logic inside controllers.
-   No static mutable state.
-   No direct access to Infrastructure from Presentation.
-   Everything asynchronous.
-   UTC timestamps only.
-   Configuration via environment variables only.
-   Every external dependency behind an interface.

------------------------------------------------------------------------

# Next Document

**03-SolutionStructure.md**

Will define:

-   Complete project layout
-   Folder structure
-   Naming conventions
-   Dependency injection
-   File naming
-   Feature organization
-   Program.cs philosophy
-   Shared kernel strategy
