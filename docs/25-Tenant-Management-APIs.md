# Kromic Store Backend Implementation Guide

# Phase 03 -- 25 Tenant Management APIs

**Version:** 1.0\
**Status:** Approved Foundation

------------------------------------------------------------------------

# Purpose

This document defines the APIs used by tenant administrators to
configure and manage their online stores.

------------------------------------------------------------------------

# Authorization

Required Role:

-   TenantAdmin

Some endpoints may also allow:

-   StoreManager (read-only or limited updates)

------------------------------------------------------------------------

# Endpoint Catalog

  Method   Endpoint                        Description
  -------- ------------------------------- --------------------------
  GET      /api/v1/tenant                  Get current tenant
  PUT      /api/v1/tenant                  Update store information
  GET      /api/v1/tenant/branding         Get branding
  PUT      /api/v1/tenant/branding         Update branding
  GET      /api/v1/tenant/settings         Get settings
  PUT      /api/v1/tenant/settings         Update settings
  GET      /api/v1/tenant/domains          List domains
  POST     /api/v1/tenant/domains          Add domain
  DELETE   /api/v1/tenant/domains/{id}     Remove domain
  POST     /api/v1/tenant/domains/verify   Verify domain
  GET      /api/v1/tenant/social-links     List social links
  PUT      /api/v1/tenant/social-links     Update social links
  GET      /api/v1/subscription            Subscription details

------------------------------------------------------------------------

# Store Information

Editable fields:

-   Store name
-   Display name
-   About us
-   Contact email
-   Contact phone
-   Default currency
-   Time zone
-   Default language

Validation:

-   Store name required
-   Currency must be supported
-   Email format validated

------------------------------------------------------------------------

# Branding

Supported assets:

-   Logo
-   Favicon
-   Banner
-   Open Graph image

Brand configuration:

-   Primary color
-   Secondary color
-   Accent color
-   Typography (future)

Uploads use Cloudinary.

------------------------------------------------------------------------

# Tenant Settings

Settings include:

-   Store visibility
-   Allow guest checkout
-   Enable reviews
-   Enable wishlists
-   Enable inventory tracking
-   Maintenance mode

------------------------------------------------------------------------

# Domain Management

Supported:

-   Subdomains
-   Custom domains

Validation:

-   Reserved names blocked
-   Unique across platform
-   Ownership verification required

------------------------------------------------------------------------

# Social Links

Supported platforms:

-   Facebook
-   Instagram
-   X
-   LinkedIn
-   YouTube
-   WhatsApp

Only valid URLs accepted.

------------------------------------------------------------------------

# Subscription

Response includes:

-   Plan
-   Status
-   Billing cycle
-   Renewal date
-   Feature limits
-   Usage summary

------------------------------------------------------------------------

# Live Preview

Tenant updates should support preview mode.

Flow:

1.  Save draft changes
2.  Preview storefront
3.  Publish configuration

Future enhancement:

-   Real-time preview via SignalR/WebSockets.

------------------------------------------------------------------------

# Validation Rules

-   Branding images must meet size limits.
-   Colors must be valid hex values.
-   Domains require verification before activation.
-   Settings changes are audited.

------------------------------------------------------------------------

# Audit

Track:

-   Who changed settings
-   Previous values
-   Timestamp
-   IP address (optional)

------------------------------------------------------------------------

# Testing

Verify:

-   Store updates
-   Branding uploads
-   Domain verification
-   Settings persistence
-   Social link validation
-   Subscription retrieval

------------------------------------------------------------------------

# Next Document

**26-Theme-Engine-APIs.md**

Topics:

-   Theme CRUD
-   Theme cloning
-   Theme publishing
-   Theme editor
-   Section management
-   Live preview
-   Theme assignment
