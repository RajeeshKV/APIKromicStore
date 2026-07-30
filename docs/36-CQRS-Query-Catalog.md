# Kromic Store Backend Implementation Guide

# Phase 03 -- 36 CQRS Query Catalog

**Version:** 1.0\
**Status:** Approved Foundation

------------------------------------------------------------------------

# Purpose

Define the complete read-side architecture of Kromic Store using CQRS.

Goals:

-   Fast, optimized read operations
-   Predictable query behavior
-   Read models independent of domain entities
-   Consistent pagination, filtering, and sorting
-   Cache-friendly design

------------------------------------------------------------------------

# Folder Structure

``` text
Application/
└── Features/
    ├── Identity/
    │   ├── Queries/
    │   ├── Handlers/
    │   └── DTOs/
    ├── Tenant/
    ├── Themes/
    ├── Catalog/
    ├── Customers/
    ├── Cart/
    ├── Checkout/
    ├── Orders/
    ├── Dashboard/
    └── Admin/
```

------------------------------------------------------------------------

# Naming Conventions

Queries:

-   GetProductQuery
-   GetProductsQuery
-   GetOrderQuery
-   GetDashboardQuery

Handlers:

-   GetProductQueryHandler
-   GetOrdersQueryHandler

DTOs:

-   ProductDto
-   OrderSummaryDto
-   CustomerProfileDto

------------------------------------------------------------------------

# Query Pipeline

``` text
Controller
    ↓
Authorization
    ↓
Validation
    ↓
Query Handler
    ↓
Projection
    ↓
DTO Response
```

Read queries never open transactions.

------------------------------------------------------------------------

# Query Guidelines

-   Use `AsNoTracking()`
-   Project directly to DTOs
-   Avoid unnecessary `Include()`
-   Never return EF entities
-   Paginate large collections
-   Apply tenant filtering automatically

------------------------------------------------------------------------

# Pagination

Default page size:

-   20

Maximum page size:

-   100

Response shape:

``` json
{
  "items": [],
  "page": 1,
  "pageSize": 20,
  "totalRecords": 250,
  "totalPages": 13
}
```

------------------------------------------------------------------------

# Filtering

Support filters where appropriate:

-   Search text
-   Status
-   Category
-   Date range
-   Price range
-   Customer
-   Tags

------------------------------------------------------------------------

# Sorting

Supported examples:

-   Name
-   CreatedOnUtc
-   UpdatedOnUtc
-   Price
-   Popularity

------------------------------------------------------------------------

# Query Catalog

## Identity

-   GetCurrentUserQuery
-   GetUserRolesQuery

## Tenant

-   GetTenantQuery
-   GetBrandingQuery
-   GetSettingsQuery
-   GetSubscriptionQuery

## Themes

-   GetThemesQuery
-   GetThemeQuery
-   GetThemePreviewQuery
-   GetThemePagesQuery

## Catalog

-   GetProductsQuery
-   GetProductQuery
-   GetCategoriesQuery
-   GetCollectionsQuery
-   SearchProductsQuery
-   GetInventoryQuery

## Customers

-   GetCustomerProfileQuery
-   GetCustomerAddressesQuery
-   GetWishlistQuery
-   GetOrderHistoryQuery

## Cart & Checkout

-   GetCartQuery
-   GetCheckoutSessionQuery
-   GetShippingMethodsQuery

## Orders

-   GetOrdersQuery
-   GetOrderQuery
-   GetOrderTimelineQuery
-   GetInvoiceQuery

## Dashboard

-   GetDashboardSummaryQuery
-   GetRevenueAnalyticsQuery
-   GetInventoryAnalyticsQuery
-   GetCustomerAnalyticsQuery

## Super Admin

-   GetTenantsQuery
-   GetPlatformAnalyticsQuery
-   GetAuditLogsQuery
-   GetFeatureFlagsQuery

------------------------------------------------------------------------

# Read Models

Use lightweight DTOs tailored to each endpoint.

Examples:

-   ProductCardDto
-   ProductDetailsDto
-   DashboardSummaryDto
-   OrderTimelineDto

Avoid over-fetching fields.

------------------------------------------------------------------------

# Caching

Recommended cache targets:

-   Store configuration
-   Public themes
-   Categories
-   Dashboard summaries (short-lived)

Future:

-   Redis distributed cache

------------------------------------------------------------------------

# Performance

Recommendations:

-   Prefer projections over Includes
-   Use indexes for filter columns
-   Avoid N+1 queries
-   Profile slow queries
-   Use compiled queries where beneficial

------------------------------------------------------------------------

# Testing

Verify:

-   Pagination
-   Filtering
-   Sorting
-   Tenant isolation
-   Authorization
-   DTO projections
-   Performance under load

------------------------------------------------------------------------

# Next Document

**37-Validation-and-Error-Handling.md**

Topics:

-   FluentValidation
-   Pipeline behaviors
-   Exception handling
-   Problem Details
-   Error codes
-   Localization
