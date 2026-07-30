# Kromic Store Backend Implementation Guide

# Phase 02 -- 14 Catalog Database

**Version:** 1.0\
**Status:** Approved Foundation

------------------------------------------------------------------------

# Purpose

Define the catalog model used by every storefront.

Goals:

-   Flexible catalog
-   High performance
-   Future-proof
-   Multi-tenant isolation
-   SEO friendly

------------------------------------------------------------------------

# Entity Overview

``` text
Category
 ├── CategorySeo
 ├── Product
 │     ├── ProductImage
 │     ├── ProductVariant
 │     ├── ProductAttribute
 │     ├── ProductTag
 │     ├── ProductCollection
 │     └── Inventory
```

------------------------------------------------------------------------

# Category

Columns:

-   Id
-   TenantId
-   ParentCategoryId (nullable)
-   Name
-   Slug
-   Description
-   DisplayOrder
-   IsVisible
-   Status

Rules:

-   Unlimited nesting
-   Slug unique per tenant
-   Parent cannot reference itself

Indexes:

-   UX_Category_Tenant_Slug
-   IX_Category_Parent

------------------------------------------------------------------------

# Product

Columns:

-   Id
-   TenantId
-   CategoryId
-   SKU
-   Name
-   Slug
-   ShortDescription
-   Description
-   ProductType (Physical/Digital)
-   Status (Draft, Active, Archived)
-   Price
-   CompareAtPrice
-   CostPrice
-   Weight
-   Length
-   Width
-   Height
-   IsFeatured
-   TrackInventory
-   Taxable

Rules:

-   SKU unique per tenant
-   Slug unique per tenant
-   Draft products are hidden

Indexes:

-   UX_Product_Tenant_SKU
-   UX_Product_Tenant_Slug
-   IX_Product_Category
-   IX_Product_Status

------------------------------------------------------------------------

# ProductImage

Columns:

-   ProductId
-   Url
-   AltText
-   DisplayOrder
-   IsPrimary

Rules:

-   Multiple images supported
-   Exactly one primary image

------------------------------------------------------------------------

# ProductVariant

Examples:

-   Size
-   Color
-   Storage
-   Material

Columns:

-   ProductId
-   SKU
-   Name
-   PriceAdjustment
-   StockQuantity
-   IsActive

Rules:

-   Variant SKU unique per tenant

------------------------------------------------------------------------

# ProductAttribute

Stores dynamic key/value pairs.

Examples:

-   Fabric
-   Brand
-   Capacity
-   Warranty

Columns:

-   ProductId
-   AttributeName
-   AttributeValue

------------------------------------------------------------------------

# Inventory

Columns:

-   ProductId
-   AvailableQuantity
-   ReservedQuantity
-   ReorderLevel

Available Stock:

AvailableQuantity - ReservedQuantity

------------------------------------------------------------------------

# ProductTag

Columns:

-   ProductId
-   Tag

Examples:

-   New
-   Trending
-   Organic
-   Handmade

------------------------------------------------------------------------

# ProductCollection

Logical grouping.

Examples:

-   Summer Collection
-   Sale
-   New Arrivals
-   Featured

Many-to-many with Products.

------------------------------------------------------------------------

# SEO

Each product/category supports:

-   MetaTitle
-   MetaDescription
-   CanonicalUrl
-   OpenGraphImage

------------------------------------------------------------------------

# Search

Searchable fields:

-   Name
-   SKU
-   Description
-   Tags
-   Attributes

Future:

-   PostgreSQL Full Text Search
-   Elasticsearch

------------------------------------------------------------------------

# Business Rules

-   Every product belongs to one category.
-   Images are stored in Cloudinary.
-   Variants are optional.
-   Products may exist without variants.
-   Soft delete all catalog entities.

------------------------------------------------------------------------

# Recommended Indexes

-   (TenantId, SKU)
-   (TenantId, Slug)
-   (TenantId, CategoryId)
-   (TenantId, Status)
-   (TenantId, IsFeatured)

------------------------------------------------------------------------

# Testing

Verify:

-   Category hierarchy
-   SKU uniqueness
-   Slug uniqueness
-   Variant pricing
-   Primary image validation
-   Inventory calculations
-   Search filters

------------------------------------------------------------------------

# Future Enhancements

-   Bundles
-   Gift cards
-   Subscription products
-   Product reviews
-   AI-generated descriptions
-   Barcode support

------------------------------------------------------------------------

# Next Document

**15-Customer-Database.md**

Topics:

-   Customers
-   Addresses
-   Customer preferences
-   Saved addresses
-   Customer groups
-   Customer activity
