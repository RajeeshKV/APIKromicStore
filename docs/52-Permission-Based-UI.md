# Kromic Store Frontend Documentation

# Phase 04 -- 52 Permission-Based UI

**Version:** 1.0\
**Status:** Approved Foundation

------------------------------------------------------------------------

# Purpose

Define how the frontend renders features based on user roles,
permissions, subscriptions, and feature flags while maintaining a secure
and consistent user experience.

------------------------------------------------------------------------

# Core Principles

-   Backend is the source of truth.
-   Frontend improves user experience by hiding unavailable features.
-   Never rely solely on frontend authorization.
-   Authorization should be centralized and reusable.

------------------------------------------------------------------------

# Authorization Layers

1.  Authentication
2.  Role-Based Access Control (RBAC)
3.  Permission-Based Access
4.  Subscription Features
5.  Feature Flags
6.  Tenant Configuration

------------------------------------------------------------------------

# Supported Roles

## Super Admin

Full platform access.

## Tenant Owner

Full access to tenant resources.

## Store Manager

Operational management with configurable permissions.

## Staff

Limited access to assigned modules.

## Customer

Access to storefront and personal account only.

------------------------------------------------------------------------

# Permission Categories

-   Dashboard
-   Store Settings
-   Theme Builder
-   Products
-   Categories
-   Inventory
-   Customers
-   Orders
-   Marketing
-   Reports
-   Billing
-   Users & Roles
-   Integrations

Each category should expose granular permissions such as View, Create,
Update, Delete, Export and Manage.

------------------------------------------------------------------------

# UI Rendering

Components should evaluate:

-   Current user
-   Assigned permissions
-   Tenant status
-   Subscription plan
-   Feature flags

Examples:

-   Hide "Delete Product" without delete permission.
-   Disable analytics if the plan doesn't include reports.
-   Hide beta features unless enabled.

------------------------------------------------------------------------

# Route Protection

Protect routes by:

-   Authentication
-   Required role
-   Required permission
-   Subscription entitlement
-   Tenant state

Unauthorized users should be redirected to an appropriate page.

------------------------------------------------------------------------

# Component Authorization

Provide reusable authorization helpers for:

-   Buttons
-   Menu items
-   Sections
-   Dialog actions
-   Navigation links

Avoid duplicating permission logic.

------------------------------------------------------------------------

# Feature Flags

Support:

-   Global platform flags
-   Tenant-specific flags
-   Beta features
-   Experimental UI

Flags should be fetched during application initialization.

------------------------------------------------------------------------

# Subscription Awareness

UI should react to subscription capabilities.

Examples:

-   Premium themes
-   Advanced analytics
-   Marketing automation
-   Additional staff seats

Show upgrade prompts where appropriate.

------------------------------------------------------------------------

# Unauthorized States

When access is denied:

-   Explain why
-   Hide destructive actions
-   Offer upgrade/request access when applicable
-   Avoid broken navigation

------------------------------------------------------------------------

# Accessibility

Hidden features should not remain keyboard focusable.

Disabled controls should communicate why they are unavailable.

------------------------------------------------------------------------

# Testing

Verify:

-   Role changes
-   Permission changes
-   Subscription upgrades
-   Feature flag toggles
-   Route protection
-   Component visibility

------------------------------------------------------------------------

# Best Practices

-   Centralize permission checks.
-   Prefer composition over scattered conditionals.
-   Keep authorization logic predictable.
-   Synchronize permission changes without page reload when possible.

------------------------------------------------------------------------

# Next Document

**53-State-Management.md**

Topics:

-   Zustand architecture
-   TanStack Query strategy
-   React Hook Form integration
-   Global state
-   Server state
-   UI state
-   Caching
-   Persistence
