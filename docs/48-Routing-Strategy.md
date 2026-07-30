# Kromic Store Frontend Documentation

# Phase 04 -- 48 Routing Strategy

**Version:** 1.0\
**Status:** Approved Foundation

------------------------------------------------------------------------

# Purpose

Define the routing architecture for Kromic Store to provide a scalable,
secure, and maintainable navigation structure across the Super Admin
Portal, Tenant Admin Portal, Theme Builder, and Public Storefront.

------------------------------------------------------------------------

# Goals

-   Modular route organization
-   Secure protected routes
-   SEO-friendly storefront URLs
-   Lazy-loaded feature modules
-   Layout-based routing
-   Permission-aware navigation

------------------------------------------------------------------------

# Routing Technology

-   React Router
-   Route-level lazy loading
-   Suspense
-   Dynamic imports

------------------------------------------------------------------------

# Route Organization

``` text
src/
└── routes/
    ├── index.tsx
    ├── public/
    ├── auth/
    ├── admin/
    ├── tenant/
    ├── storefront/
    └── shared/
```

Each feature owns its child routes.

------------------------------------------------------------------------

# Public Routes

Accessible without authentication.

Examples:

-   Landing Page
-   Login
-   Register
-   Forgot Password
-   Reset Password
-   Email Verification
-   Storefront
-   Product Details
-   Category
-   Search

------------------------------------------------------------------------

# Protected Routes

Require authentication.

## Super Admin

-   Dashboard
-   Tenants
-   Platform Settings
-   Theme Moderation
-   Analytics
-   Audit Logs

## Tenant Admin

-   Dashboard
-   Store Settings
-   Theme Builder
-   Products
-   Categories
-   Orders
-   Customers
-   Reports

------------------------------------------------------------------------

# Storefront Routes

SEO-friendly examples:

``` text
/
 /category/{slug}
 /product/{slug}
 /cart
 /checkout
 /wishlist
 /account
 /orders
 /search
```

Support configurable CMS pages:

``` text
/about
/contact
/privacy-policy
/terms
```

------------------------------------------------------------------------

# Layout-Based Routing

Layouts:

-   PublicLayout
-   AuthLayout
-   SuperAdminLayout
-   TenantAdminLayout
-   StorefrontLayout

Pages inherit navigation, headers, footers, and sidebars from their
layout.

------------------------------------------------------------------------

# Route Guards

Protect routes based on:

-   Authentication
-   User role
-   Tenant status
-   Subscription status
-   Feature flags

Unauthorized users should be redirected appropriately.

------------------------------------------------------------------------

# Lazy Loading

Lazy-load:

-   Feature modules
-   Reports
-   Analytics
-   Theme Builder
-   Product Management

Keep the initial bundle minimal.

------------------------------------------------------------------------

# Navigation Structure

Super Admin:

Dashboard → Tenants → Themes → Platform Settings → Analytics

Tenant Admin:

Dashboard → Store → Products → Orders → Customers → Reports

Storefront:

Home → Categories → Product → Cart → Checkout → Account

------------------------------------------------------------------------

# Error Pages

Provide dedicated pages for:

-   401 Unauthorized
-   403 Forbidden
-   404 Not Found
-   500 Server Error
-   Maintenance Mode

------------------------------------------------------------------------

# Breadcrumb Strategy

Automatically generate breadcrumbs from the active route hierarchy.

Support custom labels where needed.

------------------------------------------------------------------------

# Deep Linking

Support direct navigation to:

-   Product pages
-   CMS pages
-   Orders
-   Theme Builder
-   Admin pages

------------------------------------------------------------------------

# Best Practices

-   Keep routes feature-centric.
-   Avoid nested complexity.
-   Use route constants.
-   Prefer lazy loading.
-   Minimize layout duplication.
-   Preserve browser history.

------------------------------------------------------------------------

# Next Document

**49-Authentication-Flow.md**

Topics:

-   Login flow
-   Session management
-   Protected navigation
-   Refresh tokens
-   Logout
-   OAuth providers
-   Permission handling
