# Kromic Store Backend Implementation Guide

# Phase 03 -- 32 Super Admin APIs

**Version:** 1.0\
**Status:** Approved Foundation

------------------------------------------------------------------------

# Purpose

Define the APIs available exclusively to the Kromic Store platform
administrators (Super Users) for managing tenants, subscriptions,
platform configuration, and system-wide operations.

------------------------------------------------------------------------

# Authorization

  Role          Access
  ------------- ----------------------
  SuperUser     Full platform access
  TenantAdmin   No access

------------------------------------------------------------------------

# Responsibilities

Super Users can:

-   Manage tenants
-   Manage subscription plans
-   Approve and publish system themes
-   Configure platform settings
-   Manage email templates
-   View platform analytics
-   Access audit logs
-   Control maintenance mode
-   Manage feature flags

------------------------------------------------------------------------

# Tenant APIs

  Method   Endpoint                              Description
  -------- ------------------------------------- --------------------
  GET      /api/v1/admin/tenants                 List tenants
  GET      /api/v1/admin/tenants/{id}            Tenant details
  PUT      /api/v1/admin/tenants/{id}            Update tenant
  POST     /api/v1/admin/tenants/{id}/suspend    Suspend tenant
  POST     /api/v1/admin/tenants/{id}/activate   Activate tenant
  DELETE   /api/v1/admin/tenants/{id}            Soft delete tenant

Filters:

-   Status
-   Plan
-   Created date
-   Store name

------------------------------------------------------------------------

# Subscription APIs

  Method   Endpoint
  -------- ----------------------------------------
  GET      /api/v1/admin/subscriptions
  POST     /api/v1/admin/subscriptions/plans
  PUT      /api/v1/admin/subscriptions/plans/{id}
  DELETE   /api/v1/admin/subscriptions/plans/{id}

Manage:

-   Plans
-   Limits
-   Pricing
-   Trial duration
-   Features

------------------------------------------------------------------------

# Theme Moderation

  Method   Endpoint
  -------- -----------------------------------
  GET      /api/v1/admin/themes
  POST     /api/v1/admin/themes/{id}/approve
  POST     /api/v1/admin/themes/{id}/reject
  POST     /api/v1/admin/themes/{id}/publish

------------------------------------------------------------------------

# Platform Settings

Manage:

-   Contact information
-   Footer content
-   Support email
-   Branding
-   Default configuration
-   Global announcements

Endpoint:

PUT /api/v1/admin/settings

------------------------------------------------------------------------

# Email Templates

  Method   Endpoint
  -------- ---------------------------------------
  GET      /api/v1/admin/email-templates
  PUT      /api/v1/admin/email-templates/{id}
  POST     /api/v1/admin/email-templates/preview

------------------------------------------------------------------------

# Feature Flags

Examples:

-   Enable Reviews
-   Enable Coupons
-   Enable Theme Marketplace
-   Enable AI Features

Endpoint:

PUT /api/v1/admin/feature-flags

------------------------------------------------------------------------

# Platform Analytics

Metrics:

-   Total tenants
-   Active subscriptions
-   Revenue
-   Orders
-   API usage
-   Storage usage

Endpoint:

GET /api/v1/admin/analytics

------------------------------------------------------------------------

# Audit Logs

Endpoint:

GET /api/v1/admin/audit-logs

Supports filtering by:

-   User
-   Tenant
-   Action
-   Date

------------------------------------------------------------------------

# Maintenance Mode

  Method   Endpoint
  -------- -----------------------------------
  POST     /api/v1/admin/maintenance/enable
  POST     /api/v1/admin/maintenance/disable

During maintenance:

-   Storefronts may display maintenance page
-   SuperUser access remains available

------------------------------------------------------------------------

# Validation

-   Only SuperUser can access endpoints.
-   All administrative actions are audited.
-   Destructive actions require confirmation.

------------------------------------------------------------------------

# Testing

Verify:

-   Tenant lifecycle
-   Subscription updates
-   Theme approval
-   Platform settings
-   Feature flags
-   Audit logs
-   Maintenance mode

------------------------------------------------------------------------

# Next Document

**33-File-Upload-and-Cloudinary-APIs.md**

Topics:

-   Image uploads
-   Asset management
-   Cloudinary integration
-   File validation
-   CDN strategy
-   Media optimization
