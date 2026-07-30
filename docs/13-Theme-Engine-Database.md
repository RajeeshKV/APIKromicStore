# Kromic Store Backend Implementation Guide

# Phase 02 -- 13 Theme Engine Database

**Version:** 1.0\
**Status:** Approved Foundation

------------------------------------------------------------------------

# Purpose

This document defines the database design for the Kromic Store Theme
Engine.

The goal is to allow tenants to use built-in themes, clone existing
themes, or create completely custom themes without modifying frontend
code.

------------------------------------------------------------------------

# Design Goals

-   Data-driven rendering
-   Reusable themes
-   Tenant-specific customization
-   No theme versioning
-   Fast storefront rendering
-   Future extensibility

------------------------------------------------------------------------

# Entity Overview

``` text
Theme
 ├── ThemePage
 │     └── ThemeSection
 │            └── ThemeSectionItem
 ├── ThemeAsset
 └── ThemeAssignment
```

------------------------------------------------------------------------

# Theme

Represents a reusable theme.

Columns:

-   Id
-   Name
-   Description
-   IsPublic
-   IsSystemTheme
-   OwnerTenantId (nullable)
-   PreviewImageUrl
-   CreatedOnUtc
-   ModifiedOnUtc

Rules:

-   System themes cannot be edited directly.
-   Tenants clone system themes before customizing.
-   Editing creates a new theme instead of versioning.

------------------------------------------------------------------------

# ThemePage

Represents a page within a theme.

Examples:

-   Home
-   About
-   Contact
-   Products
-   Product Details
-   Cart

Columns:

-   Id
-   ThemeId
-   PageName
-   DisplayOrder

------------------------------------------------------------------------

# ThemeSection

Represents a reusable page section.

Examples:

-   Hero
-   Featured Products
-   Testimonials
-   FAQ
-   Footer
-   Header
-   Newsletter

Columns:

-   Id
-   ThemePageId
-   SectionType
-   DisplayOrder
-   IsVisible

------------------------------------------------------------------------

# ThemeSectionItem

Stores configurable content.

Examples:

-   Heading
-   Subtitle
-   Button Text
-   Banner Image
-   Background Color
-   Typography
-   Animation

Columns:

-   Id
-   ThemeSectionId
-   Key
-   Value
-   ValueType

------------------------------------------------------------------------

# ThemeAsset

Stores uploaded assets.

Columns:

-   Id
-   ThemeId
-   AssetType
-   Url

Examples:

-   Background Images
-   Icons
-   Videos
-   SVG Files

------------------------------------------------------------------------

# ThemeAssignment

Links a tenant to the active theme.

Columns:

-   TenantId
-   ThemeId
-   AssignedOnUtc

Rules:

-   One active theme per tenant.
-   Theme changes take effect immediately.

------------------------------------------------------------------------

# Rendering Strategy

Frontend downloads theme definition and renders dynamically.

Benefits:

-   No redeployment
-   Live customization
-   Shared rendering engine
-   Consistent UX

------------------------------------------------------------------------

# Indexes

-   IX_Theme_IsPublic
-   IX_Theme_OwnerTenantId
-   IX_ThemePage_ThemeId
-   IX_ThemeSection_PageId
-   IX_ThemeAssignment_TenantId (Unique)

------------------------------------------------------------------------

# Business Rules

-   Public themes are visible to all tenants.
-   Private themes belong to one tenant.
-   System themes are read-only.
-   Theme cloning creates a new record.
-   Deleting a theme in use is not allowed.

------------------------------------------------------------------------

# Future Enhancements

-   Theme marketplace
-   Premium themes
-   Theme import/export
-   AI-assisted theme generation
-   Theme analytics

------------------------------------------------------------------------

# Testing

Verify:

-   Theme cloning
-   Theme assignment
-   Dynamic rendering
-   Public/private visibility
-   Section ordering
-   Asset loading

------------------------------------------------------------------------

# Next Document

**14-Catalog-Database.md**

Topics:

-   Categories
-   Products
-   Product Images
-   Product Variants
-   Inventory
-   Pricing
-   Search
-   SEO metadata
