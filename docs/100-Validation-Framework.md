# Kromic Store Backend Documentation

# Phase 06 -- 100 Validation Framework

**Version:** 1.0\
**Status:** Approved Foundation

------------------------------------------------------------------------

# Purpose

This document defines the validation architecture for Kromic Store.
Validation ensures that requests are structurally correct, business
rules are enforced consistently, and invalid data is rejected before
reaching the domain layer.

------------------------------------------------------------------------

# Objectives

-   Validate every incoming request
-   Separate structural and business validation
-   Centralize validation logic
-   Return consistent error responses
-   Improve maintainability and security

------------------------------------------------------------------------

# Validation Layers

Validation is performed in multiple stages:

1.  Transport validation
2.  Model binding validation
3.  FluentValidation request validation
4.  Business rule validation
5.  Domain invariant validation
6.  Database constraint validation

Each layer has a distinct responsibility.

------------------------------------------------------------------------

# FluentValidation

Use FluentValidation for all Commands and Queries.

Benefits:

-   Strong typing
-   Reusable rules
-   Testability
-   Clear error messages
-   Separation from controllers

Avoid placing validation logic inside controllers.

------------------------------------------------------------------------

# MediatR Pipeline Behavior

Register a validation pipeline behavior that:

-   Executes all validators
-   Aggregates failures
-   Stops invalid requests
-   Prevents handler execution

Validation should occur before transactions begin.

------------------------------------------------------------------------

# Request Validation

Validate:

-   Required fields
-   Length limits
-   Formats
-   Enum values
-   Numeric ranges
-   Collection sizes
-   File metadata

Reject malformed requests early.

------------------------------------------------------------------------

# Business Validation

Examples:

-   Unique email addresses
-   SKU uniqueness
-   Subscription limits
-   Theme compatibility
-   Inventory availability

Business validation may require repository access.

------------------------------------------------------------------------

# Domain Validation

The domain layer must enforce invariants regardless of external
validation.

Examples:

-   Invalid state transitions
-   Negative inventory
-   Invalid aggregate relationships

Never rely solely on request validators.

------------------------------------------------------------------------

# Error Responses

Return standardized validation responses containing:

-   Error code
-   Field name
-   Message
-   Attempted value (optional)
-   Correlation ID

Avoid exposing internal implementation details.

------------------------------------------------------------------------

# Localization

Support localized validation messages using resource files.

Messages should be:

-   Human-readable
-   Consistent
-   Actionable

------------------------------------------------------------------------

# Performance

Recommendations:

-   Keep validators lightweight
-   Minimize database lookups
-   Cache reference data where appropriate
-   Avoid duplicate validation work

------------------------------------------------------------------------

# Testing

Verify:

-   Valid requests
-   Missing fields
-   Invalid formats
-   Boundary values
-   Business rule failures
-   Pipeline execution

Unit test validators independently from handlers.

------------------------------------------------------------------------

# Best Practices

-   Keep validators focused.
-   Reuse common rules.
-   Fail fast.
-   Keep domain invariants inside the domain model.
-   Standardize validation responses across the API.

------------------------------------------------------------------------

# Next Document

**101 -- Exception Handling**

Topics:

-   Global exception middleware
-   Error classification
-   Problem Details
-   Logging
-   Correlation IDs
-   Production-safe responses
