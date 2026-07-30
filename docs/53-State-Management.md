# Kromic Store Frontend Documentation

# Phase 04 -- 53 State Management

**Version:** 1.0\
**Status:** Approved Foundation

------------------------------------------------------------------------

# Purpose

Define a predictable, scalable, and maintainable state management
strategy for the Kromic Store frontend.

The architecture separates server state, client state, UI state, and
form state to improve performance, simplify maintenance, and reduce
unnecessary re-renders.

------------------------------------------------------------------------

# Goals

-   Single source of truth
-   Predictable state updates
-   Minimal component coupling
-   Excellent performance
-   Easy debugging
-   Feature-first organization

------------------------------------------------------------------------

# State Categories

## Server State

Managed using **TanStack Query**.

Examples:

-   Products
-   Categories
-   Orders
-   Customers
-   Themes
-   Dashboard statistics
-   Settings

Responsibilities:

-   Fetching
-   Caching
-   Background refetching
-   Pagination
-   Infinite queries
-   Cache invalidation

------------------------------------------------------------------------

## Client State

Managed using **Zustand**.

Examples:

-   Current user
-   Selected tenant
-   Theme mode
-   Sidebar state
-   Notification preferences
-   Wizard progress
-   Temporary UI selections

Avoid duplicating server data.

------------------------------------------------------------------------

## Form State

Managed using **React Hook Form** with **Zod** validation.

Examples:

-   Login
-   Product editor
-   Theme builder
-   Checkout
-   Customer profile

Guidelines:

-   Validate on submit by default
-   Reuse schemas between client and server where possible
-   Display inline validation messages

------------------------------------------------------------------------

## Local Component State

Use React state only for short-lived UI concerns.

Examples:

-   Modal visibility
-   Expanded panels
-   Selected tabs
-   Hover state
-   Input focus

------------------------------------------------------------------------

# Recommended Folder Structure

``` text
src/
├── store/
│   ├── auth/
│   ├── ui/
│   ├── tenant/
│   ├── preferences/
│   └── index.ts
├── services/
├── hooks/
└── features/
```

Feature-specific state should remain inside the owning feature whenever
possible.

------------------------------------------------------------------------

# Query Organization

Organize queries by feature.

Examples:

-   products
-   categories
-   orders
-   customers
-   themes
-   dashboard

Each feature should expose:

-   query keys
-   query hooks
-   mutation hooks
-   cache helpers

------------------------------------------------------------------------

# Query Keys

Use stable hierarchical keys.

Examples:

-   \["products"\]
-   \["products", id\]
-   \["orders"\]
-   \["orders", id\]
-   \["customers"\]
-   \["dashboard"\]

Avoid string concatenation.

------------------------------------------------------------------------

# Cache Strategy

Recommended defaults:

-   Cache frequently accessed reference data.
-   Invalidate affected queries after mutations.
-   Use optimistic updates where appropriate.
-   Refetch stale dashboard data automatically.

------------------------------------------------------------------------

# Optimistic Updates

Suitable for:

-   Wishlist
-   Cart
-   Product status
-   Theme preferences
-   User settings

Rollback changes on API failure.

------------------------------------------------------------------------

# Persistence

Persist only non-sensitive preferences.

Examples:

-   Theme mode
-   Sidebar collapsed state
-   Language
-   Recently used filters

Never persist authentication secrets.

------------------------------------------------------------------------

# Error Handling

Centralize handling for:

-   API failures
-   Validation errors
-   Network interruptions
-   Unauthorized responses

Display consistent toast notifications where appropriate.

------------------------------------------------------------------------

# Performance Guidelines

-   Keep stores small.
-   Avoid global state for feature-specific data.
-   Memoize selectors.
-   Prefer derived state over duplication.
-   Prevent unnecessary re-renders.

------------------------------------------------------------------------

# Testing

Verify:

-   Store updates
-   Query invalidation
-   Optimistic updates
-   Form validation
-   Persistence
-   Error recovery

------------------------------------------------------------------------

# Best Practices

-   Server state belongs in TanStack Query.
-   Client UI state belongs in Zustand.
-   Form state belongs in React Hook Form.
-   Keep React component state minimal.
-   Design stores around features, not pages.

------------------------------------------------------------------------

# Next Document

**54-API-Layer.md**

Topics:

-   Axios configuration
-   API client
-   Request pipeline
-   Response handling
-   Authentication interceptors
-   Error mapping
-   Retry strategy
-   File uploads
-   API abstractions
