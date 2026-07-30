# Kromic Store Backend Documentation

# Phase 06 -- 89 Tenant Resolution

**Version:** 1.0\
**Status:** Approved Foundation

------------------------------------------------------------------------

# Purpose

This document defines how Kromic Store identifies the active tenant for
every incoming request. Tenant resolution is the first business
operation performed after the request enters the middleware pipeline and
is critical for security, data isolation, and routing.

------------------------------------------------------------------------

# Objectives

-   Resolve the correct tenant consistently
-   Support platform and custom domains
-   Prevent cross-tenant access
-   Minimize lookup latency
-   Provide extensible resolution strategies

------------------------------------------------------------------------

# Resolution Order

Recommended priority:

1.  Platform subdomain
2.  Custom domain
3.  Trusted request header (internal use)
4.  JWT tenant claim (validation only)
5.  Explicit API route (special integrations)

Only one strategy should determine the active tenant.

------------------------------------------------------------------------

# Platform Domains

Examples:

-   store1.kromic.store
-   demo.kromic.store

The subdomain maps to a tenant record.

------------------------------------------------------------------------

# Custom Domains

Examples:

-   www.example.com
-   shop.example.com

Maintain a verified domain mapping table.

Checks:

-   Domain ownership
-   Active status
-   SSL configured
-   DNS verified

------------------------------------------------------------------------

# Internal Header Resolution

Trusted internal services may send a tenant identifier using a protected
header.

Requirements:

-   Internal network only
-   Mutual trust
-   Never expose to public clients
-   Validate against the resolved tenant

------------------------------------------------------------------------

# JWT Claims

JWT should include:

-   TenantId
-   UserId
-   Roles
-   Permissions

Claims must be validated against the resolved tenant before authorizing
requests.

------------------------------------------------------------------------

# Middleware

Create a dedicated Tenant Resolution Middleware.

Responsibilities:

-   Read request host
-   Resolve tenant
-   Validate status
-   Store tenant context
-   Continue request pipeline

Execute before authentication where host resolution is required.

------------------------------------------------------------------------

# Tenant Context

Expose a scoped context containing:

-   TenantId
-   StoreId
-   Store Name
-   Feature Flags
-   Theme
-   Locale
-   Time Zone

Inject through an abstraction such as `ITenantContext`.

------------------------------------------------------------------------

# Caching

Cache domain mappings to reduce database lookups.

Recommendations:

-   Memory cache
-   Distributed cache
-   Automatic invalidation after domain updates

------------------------------------------------------------------------

# Validation

Reject requests when:

-   Tenant not found
-   Tenant suspended
-   Domain unverified
-   Tenant archived

Return consistent problem details responses.

------------------------------------------------------------------------

# Security

-   Never trust client-supplied tenant IDs.
-   Resolve from trusted sources.
-   Validate JWT tenant ownership.
-   Audit failed resolution attempts.
-   Prevent host header spoofing.

------------------------------------------------------------------------

# Observability

Log:

-   Resolved tenant
-   Resolution strategy
-   Resolution duration
-   Failure reason

Include correlation IDs for tracing.

------------------------------------------------------------------------

# Testing

Verify:

-   Platform domain resolution
-   Custom domain resolution
-   Cache hits and misses
-   Invalid domains
-   Suspended tenants
-   JWT mismatch scenarios

------------------------------------------------------------------------

# Best Practices

-   Resolve the tenant once per request.
-   Cache verified mappings.
-   Keep resolution middleware lightweight.
-   Treat tenant resolution as a security boundary.
-   Fail fast when tenant validation fails.

------------------------------------------------------------------------

# Next Document

**90 -- Tenant Provisioning**

Topics:

-   Tenant creation
-   Initial setup
-   Default data
-   Theme assignment
-   Domain registration
-   Provisioning workflow
