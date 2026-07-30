# Kromic Store Backend Documentation

# Phase 06 -- 85 Clean Architecture

**Version:** 1.0\
**Status:** Approved Foundation

------------------------------------------------------------------------

# Purpose

This document defines the Clean Architecture principles adopted by
Kromic Store. The architecture ensures business rules remain independent
of frameworks, databases, UI technologies, and external services, making
the platform easier to maintain, test, and evolve.

------------------------------------------------------------------------

# Objectives

-   Separate business rules from implementation details
-   Enable independent testing of each layer
-   Reduce coupling
-   Improve maintainability
-   Support long-term scalability

------------------------------------------------------------------------

# Core Principles

-   Dependency Inversion
-   Separation of Concerns
-   Single Responsibility
-   Explicit Dependencies
-   Framework Independence
-   Testability

------------------------------------------------------------------------

# Layer Overview

## Domain

The innermost layer containing:

-   Entities
-   Aggregates
-   Value Objects
-   Domain Services
-   Domain Events
-   Business Rules

The Domain layer must never reference any other project.

------------------------------------------------------------------------

## Application

Responsible for implementing use cases.

Contains:

-   Commands
-   Queries
-   Handlers
-   Validators
-   DTOs
-   Interfaces
-   Pipeline Behaviors

Coordinates business workflows without containing infrastructure
details.

------------------------------------------------------------------------

## Infrastructure

Provides implementations for:

-   Persistence
-   Email
-   Storage
-   Payments
-   Background Jobs
-   External APIs
-   Caching

Implements interfaces defined in the Application layer.

------------------------------------------------------------------------

## API

Exposes functionality through HTTP.

Responsibilities:

-   Authentication
-   Authorization
-   Endpoint definitions
-   Middleware
-   Request validation
-   Response formatting
-   API documentation

------------------------------------------------------------------------

# Dependency Rule

Dependencies always point inward.

    API
     ↓
    Application
     ↓
    Domain

    Infrastructure ─────► Application
    Infrastructure ─────► Domain

The Domain layer must never depend on Application, Infrastructure, or
API.

------------------------------------------------------------------------

# Use Case Boundaries

Each business capability should be implemented as an independent use
case.

Example features:

-   Create Product
-   Publish Theme
-   Place Order
-   Update Inventory
-   Register Customer

Each use case should expose a single command or query.

------------------------------------------------------------------------

# Dependency Injection

Register infrastructure implementations at application startup.

Examples:

-   Repositories
-   Email providers
-   Payment providers
-   File storage
-   Cache providers

Consume abstractions rather than concrete implementations.

------------------------------------------------------------------------

# Domain Purity

The Domain layer must not reference:

-   Entity Framework Core
-   ASP.NET Core
-   Logging frameworks
-   HTTP clients
-   Cloud SDKs

Business rules should remain portable.

------------------------------------------------------------------------

# Interface Placement

Interfaces belong in the Application layer when they define required
behavior.

Examples:

-   IProductRepository
-   IEmailService
-   ICurrentUserService
-   IFileStorage

Infrastructure provides implementations.

------------------------------------------------------------------------

# Cross-Cutting Concerns

Implement using middleware or pipeline behaviors:

-   Validation
-   Logging
-   Auditing
-   Authorization
-   Transactions
-   Performance monitoring
-   Exception handling

Avoid duplicating this logic in handlers.

------------------------------------------------------------------------

# Testing Strategy

-   Unit test Domain independently.
-   Unit test Application with mocked dependencies.
-   Integration test Infrastructure.
-   End-to-end test the API.

------------------------------------------------------------------------

# Common Anti-Patterns

Avoid:

-   Business logic inside controllers
-   Domain objects referencing EF Core
-   Direct database access from API
-   Circular dependencies
-   Large "utility" classes

------------------------------------------------------------------------

# Best Practices

-   Keep controllers thin.
-   Keep handlers focused on a single use case.
-   Protect domain integrity.
-   Depend on abstractions.
-   Keep infrastructure replaceable.

------------------------------------------------------------------------

# Next Document

**86 -- CQRS & MediatR**

Topics:

-   Commands
-   Queries
-   Handlers
-   Pipeline Behaviors
-   Validation
-   Notifications
-   Request lifecycle
