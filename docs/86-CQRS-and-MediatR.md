# Kromic Store Backend Documentation

# Phase 06 -- 86 CQRS & MediatR

**Version:** 1.0\
**Status:** Approved Foundation

------------------------------------------------------------------------

# Purpose

This document defines how Command Query Responsibility Segregation
(CQRS) and MediatR are implemented throughout Kromic Store. Every
business capability should be modeled as an explicit command or query to
improve maintainability, scalability, and testability.

------------------------------------------------------------------------

# Objectives

-   Separate reads from writes
-   Isolate business use cases
-   Improve code organization
-   Enable reusable pipeline behaviors
-   Simplify testing and maintenance

------------------------------------------------------------------------

# CQRS Overview

CQRS divides application logic into two categories:

-   **Commands** --- modify application state.
-   **Queries** --- retrieve data without changing state.

Commands and queries should never share handlers.

------------------------------------------------------------------------

# Commands

Commands represent an intention to change the system.

Examples:

-   CreateProduct
-   UpdateInventory
-   PublishTheme
-   PlaceOrder
-   CancelOrder
-   RegisterCustomer

Guidelines:

-   One command = one business action
-   Return minimal data
-   Validate before execution
-   Be idempotent where appropriate

------------------------------------------------------------------------

# Queries

Queries retrieve information only.

Examples:

-   GetProductById
-   SearchProducts
-   GetTenantDashboard
-   GetOrderHistory
-   GetCustomerProfile

Guidelines:

-   Never modify data
-   Optimize for read performance
-   Support filtering, sorting, and pagination

------------------------------------------------------------------------

# Handlers

Every request has exactly one handler.

Responsibilities:

-   Coordinate the use case
-   Invoke domain logic
-   Call repositories/services
-   Return a response DTO

Avoid placing unrelated business logic in a handler.

------------------------------------------------------------------------

# Request Lifecycle

1.  API receives request
2.  Model binding
3.  Authentication & Authorization
4.  MediatR dispatch
5.  Pipeline behaviors
6.  Validation
7.  Handler execution
8.  Domain logic
9.  Persistence
10. Response mapping
11. HTTP response

------------------------------------------------------------------------

# Pipeline Behaviors

Implement reusable behaviors for:

-   Validation
-   Logging
-   Performance timing
-   Authorization
-   Transactions
-   Auditing
-   Exception handling

Behaviors should execute consistently for every request.

------------------------------------------------------------------------

# Validation

Use FluentValidation.

Rules:

-   Validate input before handlers execute
-   Keep validation outside handlers
-   Return consistent validation responses
-   Avoid duplicate validation logic

------------------------------------------------------------------------

# Transactions

Commands that modify data should execute within transactional
boundaries.

Recommendations:

-   Commit only after successful execution
-   Roll back on failure
-   Keep transactions short
-   Avoid long-running external calls inside transactions

------------------------------------------------------------------------

# Notifications & Domain Events

Use notifications for:

-   Email sending
-   Audit events
-   Search indexing
-   Cache invalidation
-   Analytics

Prefer asynchronous processing for non-critical work.

------------------------------------------------------------------------

# Idempotency

Protect commands such as:

-   Payment processing
-   Order creation
-   Webhook processing

Use request identifiers or idempotency keys to prevent duplicate
execution.

------------------------------------------------------------------------

# Folder Structure

Example:

    Products/
    ├── Commands/
    ├── Queries/
    ├── Validators/
    ├── DTOs/
    └── Mappings/

Organize by feature rather than by technical type.

------------------------------------------------------------------------

# Testing

-   Unit test handlers independently
-   Mock infrastructure dependencies
-   Validate pipeline behaviors
-   Integration test end-to-end command execution

------------------------------------------------------------------------

# Common Anti-Patterns

Avoid:

-   Shared command/query handlers
-   Business logic inside controllers
-   Direct DbContext usage in API
-   Large "god" handlers
-   Side effects inside queries

------------------------------------------------------------------------

# Best Practices

-   Keep commands focused.
-   Keep queries read-only.
-   Reuse pipeline behaviors.
-   Model every business capability explicitly.
-   Prefer asynchronous processing for secondary work.

------------------------------------------------------------------------

# Next Document

**87 -- Domain Model**

Topics:

-   Entities
-   Aggregates
-   Value Objects
-   Domain Services
-   Domain Events
-   Specifications
-   Invariants
