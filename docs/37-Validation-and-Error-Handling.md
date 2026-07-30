# Kromic Store Backend Implementation Guide

# Phase 03 -- 37 Validation and Error Handling

**Version:** 1.0\
**Status:** Approved Foundation

------------------------------------------------------------------------

# Purpose

Define the validation, exception handling, and standardized error
response strategy for Kromic Store.

Goals:

-   Consistent validation
-   Predictable API errors
-   Centralized exception handling
-   Secure error responses
-   Better developer experience

------------------------------------------------------------------------

# Architecture

``` text
HTTP Request
      ↓
Authentication
      ↓
Authorization
      ↓
Validation Pipeline
      ↓
Command / Query Handler
      ↓
Global Exception Middleware
      ↓
Standardized API Response
```

------------------------------------------------------------------------

# Validation Strategy

Use **FluentValidation** for all Commands and Queries.

Validation should cover:

-   Required fields
-   String lengths
-   Email formats
-   Phone formats
-   Enum values
-   Date ranges
-   Business constraints (where appropriate)

Never place validation inside controllers.

------------------------------------------------------------------------

# MediatR Pipeline

Validation occurs before handlers execute.

Pipeline order:

1.  Logging
2.  Authorization
3.  Validation
4.  Performance Monitoring
5.  Transaction (Commands only)
6.  Handler

------------------------------------------------------------------------

# Exception Handling

A global exception middleware should translate exceptions into HTTP
responses.

Never expose:

-   Stack traces
-   Database details
-   Connection strings
-   Internal implementation details

------------------------------------------------------------------------

# Standard Error Response

Use a consistent response shape.

``` json
{
  "success": false,
  "message": "Validation failed.",
  "errors": [
    "Product name is required."
  ],
  "traceId": "00-xxxxxxxx"
}
```

------------------------------------------------------------------------

# RFC 7807

Internally support Problem Details concepts while maintaining the
standard API response format.

Include:

-   Status
-   Title
-   Trace Identifier

------------------------------------------------------------------------

# Exception Mapping

  Exception                       HTTP Status
  ----------------------------- -------------
  ValidationException                     400
  UnauthorizedAccessException             401
  ForbiddenException                      403
  NotFoundException                       404
  ConflictException                       409
  BusinessRuleException                   422
  ConcurrencyException                    409
  UnexpectedException                     500

------------------------------------------------------------------------

# Correlation IDs

Every request should have a correlation identifier.

Sources:

-   Incoming request header
-   Generated automatically if missing

Include the value in:

-   Logs
-   Responses
-   Outbound HTTP requests

------------------------------------------------------------------------

# Logging

Log:

-   Request path
-   User
-   Tenant
-   Duration
-   Status code
-   Exception type
-   Correlation ID

Never log:

-   Passwords
-   Tokens
-   Payment secrets

------------------------------------------------------------------------

# Localization

Validation messages should support localization.

Default:

-   English

Future:

-   Multiple languages

------------------------------------------------------------------------

# Business Rules

Business rule failures should return HTTP 422.

Examples:

-   Product out of stock
-   Coupon expired
-   Invalid order transition
-   Duplicate domain

------------------------------------------------------------------------

# Validation Best Practices

-   One validator per Command/Query
-   Reuse common validators
-   Keep validators deterministic
-   Avoid database access unless required

------------------------------------------------------------------------

# Testing

Verify:

-   Validation failures
-   Exception mapping
-   Correlation IDs
-   Localized messages
-   Sensitive data masking
-   Middleware behavior

------------------------------------------------------------------------

# Next Document

**38-API-Versioning-and-Swagger.md**

Topics:

-   API versioning
-   OpenAPI
-   Swagger configuration
-   API documentation
-   Deprecation strategy
