# Kromic Store Backend Implementation Guide

# Phase 01 - 04 Technology Stack

**Version:** 1.0\
**Status:** Draft

------------------------------------------------------------------------

# 1. Philosophy

Every dependency included in the solution must satisfy at least one of
the following:

-   Improve maintainability
-   Improve performance
-   Improve security
-   Reduce implementation complexity
-   Support long-term scalability

Avoid unnecessary packages.

------------------------------------------------------------------------

# 2. Backend Technology Stack

## Runtime

**.NET 8 (LTS)**

Reasons:

-   Long Term Support
-   High performance
-   Excellent container support
-   Mature ASP.NET Core ecosystem
-   EF Core 8 compatibility

------------------------------------------------------------------------

## Web Framework

**ASP.NET Core Web API**

Reasons:

-   Cross-platform
-   High performance
-   Built-in Dependency Injection
-   Middleware pipeline
-   First-class OpenAPI support

------------------------------------------------------------------------

## ORM

**Entity Framework Core 8**

Decision:

For Kromic Store, EF Core is preferred over Dapper because:

-   Automatic migrations
-   Better Render deployment experience
-   Strong typing
-   Global query filters for multi-tenancy
-   Navigation properties
-   Concurrency support

Guideline:

-   Use EF Core directly inside CQRS handlers for most operations.
-   Introduce specifications or dedicated query services only when
    complexity requires them.

------------------------------------------------------------------------

## Database

**PostgreSQL**

Recommended Hosting:

-   Supabase PostgreSQL

Reasons:

-   Reliable
-   Free tier
-   Easy backups
-   Excellent EF Core support

------------------------------------------------------------------------

## Authentication

Components:

-   JWT Access Tokens
-   Refresh Tokens
-   Token Versioning
-   Email Verification
-   Google OAuth

Password Hashing:

-   ASP.NET Core Identity PasswordHasher

Reason:

Versioned hashing and smooth integration with the ASP.NET ecosystem.

------------------------------------------------------------------------

## CQRS

Library:

-   MediatR

Benefits:

-   Small controllers
-   Pipeline behaviors
-   Testability
-   Separation of commands and queries

------------------------------------------------------------------------

## Validation

Library:

-   FluentValidation

Benefits:

-   Reusable validation rules
-   Readable syntax
-   Pipeline integration

------------------------------------------------------------------------

## Object Mapping

Recommendation:

-   Mapperly

Reason:

-   Compile-time mapping
-   Better performance than reflection-based mapping
-   Type safety

------------------------------------------------------------------------

## Logging

Library:

-   Serilog

Recommended sinks:

-   Console
-   Rolling File

Future:

-   Seq
-   Application Insights
-   OpenTelemetry

------------------------------------------------------------------------

## Retry Policies

Library:

-   Polly

Use for:

-   Brevo
-   Cloudinary
-   Razorpay

Avoid retrying database operations unless a specific strategy is
implemented.

------------------------------------------------------------------------

## API Documentation

-   Swashbuckle (Swagger)

------------------------------------------------------------------------

## API Versioning

Library:

-   Asp.Versioning

Route format:

``` text
/api/v1/
```

------------------------------------------------------------------------

## Health Checks

Library:

-   Microsoft.Extensions.Diagnostics.HealthChecks

Expose:

-   GET /health
-   HEAD /health

------------------------------------------------------------------------

# 3. Infrastructure Services

## Cloudinary

Used for:

-   Tenant logos
-   Product images
-   Future marketing assets

Recommended folder convention:

``` text
tenantId/products/
tenantId/logo/
tenantId/banners/
```

------------------------------------------------------------------------

## Razorpay

Used for:

-   Tenant payments
-   Platform subscriptions
-   Refunds

Kromic Store never stores tenant secrets in plaintext.

------------------------------------------------------------------------

## Brevo

Used for:

-   Email verification
-   Order notifications
-   Password reset
-   Contact form
-   Subscription emails

------------------------------------------------------------------------

# 4. Background Processing

Hosted Services:

-   Email Outbox Worker
-   Refund Worker
-   Statistics Worker
-   Cleanup Worker

Reason for not using Hangfire initially:

Render Free Plan is sufficient with Hosted Services and keeps
infrastructure simpler.

------------------------------------------------------------------------

# 5. Frontend Technology Stack

Framework:

-   React 19

Bundler:

-   Vite

Language:

-   TypeScript

Routing:

-   React Router

Data Fetching:

-   TanStack Query

Forms:

-   React Hook Form
-   Zod

Animations:

-   Framer Motion

UI:

-   Material UI
-   Tailwind CSS

Icons:

-   Lucide

Notifications:

-   React Hot Toast

Loading:

-   React Loading Skeleton

------------------------------------------------------------------------

# 6. Docker

Strategy:

-   Multi-stage build
-   Linux containers
-   Health checks
-   Automatic EF migrations on startup
-   Graceful shutdown

------------------------------------------------------------------------

# 7. CI/CD

Recommended:

-   GitHub Actions

Pipeline:

1.  Restore
2.  Build
3.  Test
4.  Publish
5.  Build Docker Image
6.  Deploy to Render
7.  Health Check

------------------------------------------------------------------------

# 8. Monitoring

Use:

-   Serilog
-   Correlation ID
-   Structured Logging

Future additions:

-   OpenTelemetry
-   Metrics dashboard

------------------------------------------------------------------------

# 9. Future Technologies

Potential additions:

-   Redis
-   RabbitMQ
-   Elasticsearch
-   Azure Blob Storage
-   Stripe
-   Marketplace module

------------------------------------------------------------------------

# 10. Recommended NuGet Packages

  Area             Package
  ---------------- -----------------------------------------------
  CQRS             MediatR
  Validation       FluentValidation
  Mapping          Mapperly
  Logging          Serilog
  Retry            Polly
  API Docs         Swashbuckle.AspNetCore
  API Versioning   Asp.Versioning
  JWT              Microsoft.AspNetCore.Authentication.JwtBearer
  EF Core          Microsoft.EntityFrameworkCore
  PostgreSQL       Npgsql.EntityFrameworkCore.PostgreSQL
  Images           CloudinaryDotNet
  Payments         Razorpay .NET SDK
  Testing          xUnit
  Mocking          NSubstitute
  Assertions       FluentAssertions
  Containers       Testcontainers

------------------------------------------------------------------------

# Architecture Decisions

1.  Prefer Mapperly over AutoMapper.
2.  Use ASP.NET Core Identity PasswordHasher.
3.  Avoid a generic repository abstraction.
4.  Keep configuration strongly typed through Options.
5.  Place all secrets in Render environment variables.

------------------------------------------------------------------------

# Next Document

**05-CodingStandards.md**

This document will define coding conventions, handler templates, DTO
rules, controller standards, logging guidelines, testing conventions,
and implementation rules followed across the entire solution.
