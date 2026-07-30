# Kromic Store Backend Implementation Guide

# Phase 03 -- 26 Theme Engine APIs

**Version:** 1.0\
**Status:** Approved Foundation

------------------------------------------------------------------------

# Purpose

Define the API surface for Kromic Store's visual theme engine, enabling
tenants to build, customize, preview, and publish storefronts without
code.

------------------------------------------------------------------------

# Authorization

  Role           Access
  -------------- --------------------------------
  TenantAdmin    Full access
  StoreManager   Create/Edit content (optional)
  Customer       No access

------------------------------------------------------------------------

# Theme Lifecycle

``` text
Create
   ↓
Customize
   ↓
Preview
   ↓
Publish
   ↓
Assign to Store
```

------------------------------------------------------------------------

# Endpoint Catalog

  Method   Endpoint                      Description
  -------- ----------------------------- ------------------------
  GET      /api/v1/themes                List themes
  POST     /api/v1/themes                Create theme
  GET      /api/v1/themes/{id}           Theme details
  PUT      /api/v1/themes/{id}           Update theme
  DELETE   /api/v1/themes/{id}           Soft delete theme
  POST     /api/v1/themes/{id}/clone     Clone theme
  POST     /api/v1/themes/{id}/publish   Publish draft
  POST     /api/v1/themes/{id}/assign    Assign theme to tenant
  GET      /api/v1/themes/{id}/preview   Preview theme
  POST     /api/v1/themes/import         Import theme
  GET      /api/v1/themes/{id}/export    Export theme

------------------------------------------------------------------------

# Theme Metadata

Properties:

-   Name
-   Description
-   Thumbnail
-   Category
-   IsPublic
-   IsSystemTheme
-   Status (Draft/Published)
-   VersionLabel (display only)

------------------------------------------------------------------------

# Pages

Supported pages:

-   Home
-   Product Details
-   Category
-   Cart
-   Checkout
-   About Us
-   Contact Us
-   Search
-   Custom Pages

Endpoints:

-   GET /api/v1/themes/{id}/pages
-   POST /api/v1/themes/{id}/pages
-   PUT /api/v1/themes/{id}/pages/{pageId}
-   DELETE /api/v1/themes/{id}/pages/{pageId}

------------------------------------------------------------------------

# Sections

Examples:

-   Hero
-   Featured Products
-   Categories
-   Testimonials
-   Newsletter
-   Banner
-   Rich Text
-   Image Gallery
-   FAQ

CRUD APIs provided for all sections.

------------------------------------------------------------------------

# Section Items

Reusable content blocks:

-   Cards
-   Slides
-   Buttons
-   Images
-   Videos
-   Icons

Ordering is configurable using DisplayOrder.

------------------------------------------------------------------------

# Assets

Supported:

-   Images
-   Videos
-   Fonts
-   CSS
-   JavaScript (future, sandboxed)

Cloudinary stores media assets.

------------------------------------------------------------------------

# Preview

Preview endpoints return unpublished content without affecting the live
storefront.

Future:

-   Live preview over SignalR/WebSockets.

------------------------------------------------------------------------

# Publishing Rules

Before publishing:

-   Validate required pages
-   Validate broken assets
-   Validate duplicate routes
-   Ensure theme integrity

Publishing is atomic.

------------------------------------------------------------------------

# Theme Assignment

Only one active theme per tenant.

Assigning a new theme automatically deactivates the previous assignment.

------------------------------------------------------------------------

# Import / Export

Export format:

-   JSON manifest
-   Assets
-   Pages
-   Sections
-   Metadata

Future:

-   Theme marketplace packages.

------------------------------------------------------------------------

# Validation

-   Unique page routes
-   Valid section types
-   Asset existence
-   Maximum upload sizes
-   Theme name required

------------------------------------------------------------------------

# Testing

Verify:

-   Theme CRUD
-   Clone workflow
-   Publish workflow
-   Assignment
-   Preview
-   Import/export
-   Section ordering

------------------------------------------------------------------------

# Next Document

**27-Catalog-APIs.md**

Topics:

-   Categories
-   Products
-   Variants
-   Inventory
-   Collections
-   Search
-   Bulk operations
