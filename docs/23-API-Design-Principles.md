# Kromic Store Backend Implementation Guide

# Phase 03 -- 23 API Design Principles

**Version:** 1.0\
**Status:** Approved Foundation

------------------------------------------------------------------------

# Purpose

This document defines the API standards for every HTTP endpoint exposed
by Kromic Store.

Goals:

-   Consistent API design
-   Predictable request/response contracts
-   Strong versioning
-   Tenant awareness
-   Excellent developer experience

------------------------------------------------------------------------

# API Style

-   RESTful resource-oriented APIs
-   JSON request and response bodies
-   UTF-8 encoding
-   HTTPS only
-   OpenAPI (Swagger) documentation

Base URL:

``` text
/api/v1/
```

------------------------------------------------------------------------

# Versioning

Use URL versioning.

Examples:

``` text
/api/v1/products
/api/v1/orders
/api/v2/products
```

Breaking changes require a new version.

------------------------------------------------------------------------

# Resource Naming

Use plural nouns.

Examples:

-   /products
-   /categories
-   /orders
-   /customers
-   /themes

Avoid verbs in resource names.

------------------------------------------------------------------------

# HTTP Methods

  Method   Purpose
  -------- ----------------
  GET      Read
  POST     Create
  PUT      Replace
  PATCH    Partial update
  DELETE   Soft delete

------------------------------------------------------------------------

# Standard Response

Successful responses:

``` json
{
  "success": true,
  "data": {},
  "message": null,
  "errors": []
}
```

Error responses:

``` json
{
  "success": false,
  "data": null,
  "message": "Validation failed.",
  "errors": [
    "Product name is required."
  ],
  "traceId": "..."
}
```

------------------------------------------------------------------------

# Pagination

Query parameters:

``` text
?page=1&pageSize=20
```

Response metadata:

-   page
-   pageSize
-   totalRecords
-   totalPages

------------------------------------------------------------------------

# Filtering

Examples:

``` text
?status=Active
?categoryId={guid}
?search=shirt
?sort=name
?descending=true
```

------------------------------------------------------------------------

# Authentication

Protected endpoints require JWT Bearer tokens.

Public endpoints:

-   Storefront browsing
-   Login
-   Registration
-   Password reset
-   Email verification

------------------------------------------------------------------------

# Authorization

Policies should be role-based.

Examples:

-   SuperUser
-   TenantAdmin
-   StoreManager
-   Customer

------------------------------------------------------------------------

# Validation

-   FluentValidation
-   DTO validation before handlers
-   Return HTTP 400 on validation failure

------------------------------------------------------------------------

# Error Codes

  Status   Meaning
  -------- -------------------------
  200      OK
  201      Created
  204      No Content
  400      Validation Error
  401      Unauthorized
  403      Forbidden
  404      Not Found
  409      Conflict
  422      Business Rule Violation
  500      Internal Server Error

------------------------------------------------------------------------

# Idempotency

Support idempotency for critical POST operations such as payments by
accepting an `Idempotency-Key` header.

------------------------------------------------------------------------

# Tenant Resolution

Tenant context is resolved before controller execution.

Sources:

1.  Custom domain
2.  Subdomain
3.  Development override

------------------------------------------------------------------------

# Logging

Log:

-   Request path
-   Response status
-   Duration
-   Authenticated user
-   TenantId
-   Correlation ID

Never log passwords or tokens.

------------------------------------------------------------------------

# OpenAPI

Every endpoint should include:

-   Summary
-   Description
-   Request schema
-   Response schema
-   Status codes
-   Authorization requirements

------------------------------------------------------------------------

# Best Practices

-   Keep controllers thin
-   One endpoint, one responsibility
-   Use CQRS handlers
-   Return DTOs only
-   Never expose EF entities

------------------------------------------------------------------------

# Next Document

**24-Authentication-And-Authorization-APIs.md**

Topics:

-   Login
-   Registration
-   Google Sign-In
-   Refresh tokens
-   Logout
-   Password reset
-   Email verification
-   Role management
