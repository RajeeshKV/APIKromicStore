# Kromic Store Backend Implementation Guide

# Phase 03 -- 27 Catalog APIs

**Version:** 1.0\
**Status:** Approved Foundation

------------------------------------------------------------------------

# Purpose

Define the APIs for managing the product catalog, categories, inventory,
collections, and search.

------------------------------------------------------------------------

# Authorization

  Role           Access
  -------------- ----------------------
  TenantAdmin    Full
  StoreManager   Full (configurable)
  Customer       Read-only storefront

------------------------------------------------------------------------

# Endpoint Catalog

## Categories

  Method   Endpoint                          Description
  -------- --------------------------------- -----------------
  GET      /api/v1/categories                List categories
  POST     /api/v1/categories                Create category
  GET      /api/v1/categories/{id}           Get category
  PUT      /api/v1/categories/{id}           Update category
  DELETE   /api/v1/categories/{id}           Soft delete
  POST     /api/v1/categories/{id}/restore   Restore

------------------------------------------------------------------------

## Products

  Method   Endpoint                          Description
  -------- --------------------------------- -------------------
  GET      /api/v1/products                  List products
  POST     /api/v1/products                  Create product
  GET      /api/v1/products/{id}             Get product
  PUT      /api/v1/products/{id}             Update product
  DELETE   /api/v1/products/{id}             Soft delete
  POST     /api/v1/products/{id}/restore     Restore
  POST     /api/v1/products/{id}/duplicate   Duplicate product

------------------------------------------------------------------------

## Variants

-   GET /api/v1/products/{id}/variants
-   POST /api/v1/products/{id}/variants
-   PUT /api/v1/products/{id}/variants/{variantId}
-   DELETE /api/v1/products/{id}/variants/{variantId}

Examples:

-   Size
-   Color
-   Material

------------------------------------------------------------------------

## Inventory

Endpoints:

-   GET /api/v1/inventory
-   PUT /api/v1/inventory/{productId}
-   POST /api/v1/inventory/adjust

Track:

-   Available quantity
-   Reserved quantity
-   Low stock threshold

------------------------------------------------------------------------

## Images

Endpoints:

-   POST /api/v1/products/{id}/images
-   DELETE /api/v1/products/{id}/images/{imageId}
-   PUT /api/v1/products/{id}/images/order

Images stored in Cloudinary.

------------------------------------------------------------------------

## Collections

Endpoints:

-   GET /api/v1/collections
-   POST /api/v1/collections
-   PUT /api/v1/collections/{id}
-   DELETE /api/v1/collections/{id}

Examples:

-   New Arrivals
-   Best Sellers
-   Seasonal

------------------------------------------------------------------------

## Search

Supported filters:

-   Search text
-   Category
-   Collection
-   Price range
-   Availability
-   Featured
-   Tags

Sorting:

-   Name
-   Price
-   Created Date
-   Popularity

------------------------------------------------------------------------

## Bulk Operations

Supported:

-   Import CSV
-   Export CSV
-   Bulk update prices
-   Bulk update inventory
-   Bulk assign category
-   Bulk activate/deactivate
-   Bulk delete

------------------------------------------------------------------------

## SEO

Editable fields:

-   Slug
-   Meta title
-   Meta description
-   Keywords
-   Open Graph image

------------------------------------------------------------------------

## Validation

-   SKU unique within tenant
-   Slug unique within tenant
-   Price ≥ 0
-   Inventory ≥ 0
-   Category required
-   Variant combinations unique

------------------------------------------------------------------------

## Business Rules

-   Products use soft delete.
-   Duplicate products copy media and metadata.
-   Inventory adjustments are audited.
-   Search excludes deleted products.

------------------------------------------------------------------------

## Testing

Verify:

-   CRUD operations
-   Variant management
-   Inventory adjustments
-   Bulk import/export
-   Image uploads
-   Search & filtering
-   SEO updates
-   Duplicate product

------------------------------------------------------------------------

# Next Document

**28-Customer-APIs.md**

Topics:

-   Customer profile
-   Addresses
-   Wishlist
-   Preferences
-   Customer groups
-   Order history
-   Account management
