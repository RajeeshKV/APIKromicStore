# Kromic Store Backend Implementation Guide

# Phase 01 - 07 Docker and Deployment

**Version:** 1.0\
**Status:** Foundation

------------------------------------------------------------------------

# 1. Purpose

This document defines the containerization, startup, deployment, and
operational strategy for Kromic Store.

Primary targets:

-   Docker
-   Render Free Plan
-   Supabase PostgreSQL
-   Vercel (Frontend)

------------------------------------------------------------------------

# 2. Deployment Architecture

``` text
Developer
    │
GitHub
    │
Render (API)
    │
Supabase PostgreSQL
    │
Cloudinary
Brevo
Razorpay

Vercel
    │
React Storefront
```

------------------------------------------------------------------------

# 3. Docker Strategy

Use a multi-stage Docker build.

Stages:

1.  Restore
2.  Build
3.  Publish
4.  Runtime

Benefits:

-   Smaller image
-   Faster deployments
-   Better security
-   Reduced startup time

------------------------------------------------------------------------

# 4. Runtime Container

The runtime image should contain only:

-   Published application
-   Runtime dependencies
-   Health endpoint
-   Environment variables

Do not include:

-   SDK
-   Source code
-   Test artifacts

------------------------------------------------------------------------

# 5. Startup Sequence

``` text
Container Starts
      │
Load Configuration
      │
Validate Options
      │
Configure Logging
      │
Connect Database
      │
Apply EF Migrations
      │
Seed SuperUser (if missing)
      │
Start Hosted Services
      │
Expose HTTP Endpoints
```

------------------------------------------------------------------------

# 6. EF Core Migrations

Rules:

-   Apply automatically on startup.
-   Run once per deployment.
-   Fail startup if migration fails.
-   Never ignore migration errors.

Database schema is always managed by EF Core migrations.

------------------------------------------------------------------------

# 7. Health Checks

Endpoints:

-   GET /health
-   HEAD /health

Checks:

-   Database connectivity
-   Application startup

Health endpoint should return quickly.

------------------------------------------------------------------------

# 8. Graceful Shutdown

Hosted services must:

-   Observe CancellationToken
-   Finish active work where possible
-   Flush logs before exit

------------------------------------------------------------------------

# 9. Render Configuration

Build Command:

``` text
docker build
```

Start Command:

Container entrypoint.

Health Check Path:

``` text
/health
```

Secrets:

Configured only through Render Environment Variables.

------------------------------------------------------------------------

# 10. Logging

Startup logs should include:

-   Environment
-   Application version
-   Migration result
-   Listening URL

Never log:

-   Secrets
-   Passwords
-   JWT signing keys

------------------------------------------------------------------------

# 11. Docker Ignore

Exclude:

``` text
.git
.vscode
bin
obj
TestResults
node_modules
docs
```

------------------------------------------------------------------------

# 12. Local Development

Recommended services:

-   Docker Desktop
-   PostgreSQL (or Supabase)
-   User Secrets
-   Hot Reload

Frontend:

-   Vite

Backend:

-   ASP.NET Core

------------------------------------------------------------------------

# 13. Production Checklist

Before deployment:

-   Build succeeds
-   Tests pass
-   Docker image builds
-   Environment variables configured
-   Database accessible
-   Cloudinary configured
-   Brevo configured
-   Razorpay configured
-   Health endpoint returns 200
-   Swagger disabled or protected (production)

------------------------------------------------------------------------

# 14. Rollback Strategy

If deployment fails:

1.  Investigate logs.
2.  Restore previous successful deployment.
3.  Verify database compatibility.
4.  Re-run deployment after fix.

Avoid destructive migrations without backups.

------------------------------------------------------------------------

# 15. Future Improvements

Potential future additions:

-   Blue/Green deployment
-   Canary releases
-   OpenTelemetry
-   Centralized log aggregation
-   Redis cache
-   CDN for static assets

------------------------------------------------------------------------

# Phase 01 Complete

Foundation documents completed:

-   01 Vision
-   02 System Architecture
-   03 Solution Structure
-   04 Technology Stack
-   05 Coding Standards
-   06 Environment Variables
-   07 Docker and Deployment

------------------------------------------------------------------------

# Next Phase

**Phase 02 - Database Design**

This phase will define:

-   Every entity
-   Relationships
-   Foreign keys
-   Indexes
-   Audit strategy
-   Multi-tenant implementation
-   Soft delete
-   EF configurations
-   Migration strategy
-   ER diagrams
