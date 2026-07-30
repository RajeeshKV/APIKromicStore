# Kromic Store Backend Implementation Guide

# Phase 02 -- 12 Tenant and Store Database

**Version:** 1.0\
**Status:** Approved Foundation

------------------------------------------------------------------------

# Purpose

This document defines the tenant, storefront, branding, domain, and
business configuration model for Kromic Store.

The design separates concerns into focused entities instead of a single
large Tenant table.

------------------------------------------------------------------------

# Design Goals

-   Normalize store configuration
-   Support custom domains
-   Enable future expansion
-   Tenant isolation
-   Easy administration

------------------------------------------------------------------------

# Entity Overview

``` text
Tenant
 ├── TenantSubscription
 ├── TenantDomain
 ├── TenantBranding
 ├── TenantSettings
 ├── TenantSocialLinks
 └── TenantContactInformation
```

------------------------------------------------------------------------

# Tenant

Platform-level identity.

Columns:

-   Id
-   Name
-   StoreName
-   Status
-   OwnerUserId
-   CreatedOnUtc
-   ModifiedOnUtc
-   Audit Fields
-   Soft Delete Fields

Business Rules:

-   One record per business.
-   StoreName is editable.
-   Status controls platform access.

------------------------------------------------------------------------

# TenantDomain

Stores all domains belonging to a tenant.

Columns:

-   Id
-   TenantId
-   Subdomain
-   CustomDomain
-   IsPrimary
-   IsVerified

Rules:

-   Global uniqueness
-   Reserved subdomains blocked
-   Lowercase only
-   Case-insensitive lookup

Indexes:

-   UX_Subdomain
-   UX_CustomDomain

------------------------------------------------------------------------

# TenantBranding

Columns:

-   TenantId
-   LogoUrl
-   FaviconUrl
-   PrimaryColor
-   SecondaryColor
-   AccentColor
-   DefaultThemeId

Stores visual identity only.

------------------------------------------------------------------------

# TenantSettings

Columns:

-   TenantId
-   Currency
-   TimeZone
-   Language
-   OrderPrefix
-   AllowGuestCheckout
-   EnableWishlist
-   EnableReviews
-   MaintenanceMode

Future settings should be added here instead of the Tenant table.

------------------------------------------------------------------------

# TenantContactInformation

Columns:

-   TenantId
-   BusinessName
-   ContactEmail
-   Phone
-   WhatsApp
-   AddressLine1
-   AddressLine2
-   City
-   State
-   Country
-   PostalCode

Used for storefront footer, invoices and contact pages.

------------------------------------------------------------------------

# TenantSocialLinks

Columns:

-   TenantId
-   Facebook
-   Instagram
-   X
-   YouTube
-   LinkedIn
-   Pinterest

Only valid URLs should be accepted.

------------------------------------------------------------------------

# TenantSubscription

Columns:

-   TenantId
-   Plan
-   Status
-   StartsOnUtc
-   ExpiresOnUtc
-   RazorpaySubscriptionId
-   LastPaymentOnUtc

Business Rules:

-   One active subscription per tenant.
-   Expired subscriptions may limit administration access.

------------------------------------------------------------------------

# Relationships

``` text
Tenant
 ├── TenantDomain
 ├── TenantBranding
 ├── TenantSettings
 ├── TenantContactInformation
 ├── TenantSocialLinks
 └── TenantSubscription
```

------------------------------------------------------------------------

# Recommended Indexes

-   UX_Subdomain
-   UX_CustomDomain
-   IX_Tenant_Status
-   IX_Subscription_Status
-   IX_Subscription_ExpiresOnUtc

------------------------------------------------------------------------

# Business Rules

-   Tenant owns all configuration.
-   Domain changes are validated before saving.
-   Reserved subdomains cannot be used.
-   Branding changes should not affect business data.
-   Subscription changes must be audited.

------------------------------------------------------------------------

# Testing

Verify:

-   Unique subdomains
-   Reserved name validation
-   Custom domain uniqueness
-   Tenant resolution after domain update
-   Branding retrieval
-   Subscription expiry handling

------------------------------------------------------------------------

# Next Document

**13-Theme-Engine-Database.md**

Topics:

-   Theme model
-   Theme templates
-   Page sections
-   Theme assets
-   Theme publishing
-   Tenant theme assignment
