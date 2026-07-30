# Kromic Store Backend Implementation Guide

# Phase 01 - Vision and Product Scope

**Version:** 1.0\
**Status:** Approved Foundation Document

------------------------------------------------------------------------

# 1. Purpose

This document defines the vision, scope, objectives, guiding principles,
user roles, and functional boundaries of the Kromic Store SaaS platform.

This document is the source of truth for every subsequent implementation
document. No feature should be implemented unless it aligns with this
vision.

------------------------------------------------------------------------

# 2. Product Vision

Kromic Store is a multi-tenant e-commerce SaaS platform that enables
businesses to create and manage their own online storefronts without
writing code.

Every tenant owns:

-   Their own storefront
-   Their own branding
-   Their own customers
-   Their own products
-   Their own Razorpay account
-   Their own orders
-   Their own themes

Kromic acts as the platform provider and never becomes the merchant of
record.

------------------------------------------------------------------------

# 3. Core Goals

## Business Goals

-   Launch storefronts in minutes
-   Support unlimited tenants
-   Maintain strong tenant isolation
-   Be deployable on the Render Free Plan
-   Minimize operational cost
-   Keep architecture enterprise-ready

## Technical Goals

-   .NET 8
-   Clean Architecture
-   CQRS
-   JWT Authentication with Token Versioning
-   PostgreSQL (EF Core)
-   Cloudinary
-   Razorpay
-   Brevo
-   Docker
-   Render deployment
-   Environment-variable driven configuration
-   Production-ready logging and monitoring

------------------------------------------------------------------------

# 4. User Roles

## Super User

Responsibilities:

-   Platform administration
-   Tenant management
-   Subscription management
-   Public theme management
-   Platform statistics
-   Pricing management
-   Contact request monitoring

Restrictions:

-   Cannot access tenant business data except platform-level
    administration.

------------------------------------------------------------------------

## Tenant (Business User)

Responsibilities:

-   Configure store
-   Manage products
-   Manage categories
-   Configure theme
-   Configure Razorpay
-   Manage orders
-   Configure policies
-   Upload logos/images
-   Manage storefront

Restrictions:

-   Cannot access another tenant's data.

------------------------------------------------------------------------

## Customer

Responsibilities:

-   Browse products
-   Register/Login
-   Manage addresses
-   Wishlist
-   Cart
-   Place orders
-   View order history

Restrictions:

-   Can only access data belonging to the tenant they are interacting
    with.

------------------------------------------------------------------------

# 5. Multi-Tenant Principles

-   Every tenant is logically isolated.
-   All business tables contain TenantId.
-   Global query filters enforce isolation.
-   Tenant resolution occurs before controller execution.
-   No endpoint may return cross-tenant data.

------------------------------------------------------------------------

# 6. Product Modules

Platform: - Authentication - Tenant Resolution - Public Themes -
Pricing - Subscription - Notifications

Tenant: - Dashboard - Store Settings - Themes - Categories - Products -
Orders - Razorpay Configuration - Policies

Customer: - Storefront - Cart - Wishlist - Checkout - Addresses - Orders

------------------------------------------------------------------------

# 7. Functional Requirements (Summary)

Authentication - Email/password - Google OAuth - Email verification -
JWT + Refresh Token - Token versioning

Store - Categories - Products - Multiple product images - Search - Theme
rendering

Orders - Order Placed - Confirmed - Dispatched - Delivered - Manual
status updates - Email notifications - Refund on rejection

Administration - Dashboard - Analytics - Tenant management - Public
themes

------------------------------------------------------------------------

# 8. Non-Functional Requirements

Performance - Fast startup on Render - Optimized EF queries - Pagination
by default

Security - HTTPS - JWT - Correlation ID - Audit logging - Rate
limiting - Secure secrets via environment variables

Scalability - Stateless API - Background workers - Outbox pattern -
Modular services

Reliability - Retry logic for external providers - Graceful shutdown -
Health endpoints

------------------------------------------------------------------------

# 9. Success Criteria

The MVP is considered complete when:

-   A tenant can register.
-   Configure branding.
-   Upload products.
-   Configure Razorpay.
-   Publish a storefront.
-   Customers can purchase products.
-   Tenant receives notifications.
-   Orders progress through the supported lifecycle.
-   Platform administrator can manage tenants and subscriptions.

------------------------------------------------------------------------

# 10. Guiding Principles

1.  Documentation-first development.
2.  UI-driven API design.
3.  Small controllers.
4.  Business logic outside controllers.
5.  Environment-variable configuration only.
6.  Tenant isolation by design.
7.  Production-ready from day one.
8.  Favor maintainability over cleverness.
9.  Consistent API contracts.
10. Every feature must be observable through logging and metrics.

------------------------------------------------------------------------

# Next Document

**02-SystemArchitecture.md**

This document will define:

-   High-level architecture
-   Request lifecycle
-   Clean Architecture boundaries
-   CQRS flow
-   Middleware pipeline
-   Design patterns
-   Sequence diagrams
-   Dependency rules
