# Kromic Store Backend Implementation Guide

# Phase 01 - 03 Solution Structure

**Version:** 1.0\
**Part:** 2 of 3

------------------------------------------------------------------------

# 11. Program.cs Philosophy

`Program.cs` must remain minimal. It should only:

1.  Build the application.
2.  Load configuration.
3.  Register services through extension methods.
4.  Configure middleware.
5.  Apply migrations on startup.
6.  Start the application.

No business logic, option binding, or service registration should be
written inline unless absolutely necessary.

Example flow:

``` text
CreateBuilder
    ↓
Load Configuration
    ↓
Configure Serilog
    ↓
Add Application
    ↓
Add Infrastructure
    ↓
Add Authentication
    ↓
Add Swagger
    ↓
Build
    ↓
Configure Middleware
    ↓
Apply Migrations
    ↓
Run
```

------------------------------------------------------------------------

# 12. Dependency Injection Strategy

Every layer exposes a single extension method.

``` text
Api
 ├── AddPresentation()
 ├── UsePresentation()

Application
 └── AddApplication()

Infrastructure
 └── AddInfrastructure()

Persistence
 └── AddPersistence()
```

Program.cs should never contain long service registration lists.

------------------------------------------------------------------------

# 13. Extension Method Layout

``` text
KromicStore.Api/
└── DependencyInjection/
    ├── AuthenticationExtensions.cs
    ├── CorsExtensions.cs
    ├── HealthCheckExtensions.cs
    ├── MiddlewareExtensions.cs
    ├── OpenApiExtensions.cs
    ├── RateLimiterExtensions.cs
    └── VersioningExtensions.cs
```

Infrastructure follows the same convention.

------------------------------------------------------------------------

# 14. Configuration Binding

All configuration is strongly typed.

Example option classes:

``` text
JwtOptions
CloudinaryOptions
BrevoOptions
RazorpayOptions
DatabaseOptions
RenderOptions
CorsOptions
```

Every options class:

-   Uses DataAnnotations validation.
-   Is validated on startup.
-   Fails fast when invalid.

------------------------------------------------------------------------

# 15. Environment Loading

Configuration order:

1.  appsettings.json
2.  appsettings.{Environment}.json
3.  User Secrets (Development)
4.  Environment Variables
5.  Command-line arguments

Render deployments rely on environment variables for all secrets.

------------------------------------------------------------------------

# 16. Service Registration Rules

Register services by lifetime:

## Singleton

-   Configuration providers
-   Clock abstraction
-   Cloudinary client

## Scoped

-   DbContext
-   MediatR handlers
-   TenantContext
-   CurrentUser service
-   Repositories (if used)

## Transient

-   Stateless helpers
-   Mapping utilities

------------------------------------------------------------------------

# 17. Startup Validation

Before serving requests:

-   Validate required options.
-   Verify database connectivity.
-   Apply EF Core migrations.
-   Seed Super User (if absent).
-   Register health checks.

Startup should fail if critical configuration is missing.

------------------------------------------------------------------------

# 18. Health Checks

Expose:

``` text
GET /health
HEAD /health
```

Checks:

-   PostgreSQL connectivity
-   Application startup status

External services (Cloudinary, Brevo, Razorpay) should not block startup
but may expose optional health indicators later.

------------------------------------------------------------------------

# 19. Logging

Serilog should log:

-   Startup
-   Shutdown
-   Requests
-   Exceptions
-   Background workers

Every log entry should include:

-   CorrelationId
-   TenantId (when available)
-   UserId (when authenticated)

------------------------------------------------------------------------

# 20. Assembly Scanning

Use assembly scanning only where it improves maintainability.

Recommended:

-   MediatR handlers
-   FluentValidation validators

Avoid broad scanning for arbitrary services.

------------------------------------------------------------------------

# 21. Folder Responsibilities

Presentation: - HTTP concerns only

Application: - Business orchestration

Domain: - Business model

Infrastructure: - External systems and persistence

SharedKernel: - Cross-cutting reusable utilities

------------------------------------------------------------------------

# 22. Future Standards

Future additions should follow the same structure.

New modules must provide:

-   Dependency injection extension
-   Configuration options
-   Health checks (if applicable)
-   Documentation
-   Tests

------------------------------------------------------------------------

# Next

**Part 3** will cover:

-   Naming conventions
-   File naming standards
-   Namespace conventions
-   Feature templates
-   Class templates
-   Coding standards
-   Example folder trees down to class level
