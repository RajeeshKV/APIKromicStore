# Kromic Store Backend Implementation Guide

# Phase 03 -- 31 Dashboard APIs

**Version:** 1.0\
**Status:** Approved Foundation

------------------------------------------------------------------------

# Purpose

Define the analytics and dashboard APIs available to tenant
administrators for monitoring store performance.

------------------------------------------------------------------------

# Authorization

  Role           Access
  -------------- --------------------------
  TenantAdmin    Full
  StoreManager   Read-only (configurable)
  Customer       No access

------------------------------------------------------------------------

# Dashboard Overview

Endpoint:

GET /api/v1/dashboard

Returns:

-   Today's sales
-   Today's orders
-   Active customers
-   Pending orders
-   Low stock count
-   Revenue summary

------------------------------------------------------------------------

# KPI APIs

  Method   Endpoint                      Description
  -------- ----------------------------- --------------------
  GET      /api/v1/dashboard/kpis        Summary metrics
  GET      /api/v1/dashboard/revenue     Revenue analytics
  GET      /api/v1/dashboard/orders      Order analytics
  GET      /api/v1/dashboard/customers   Customer analytics
  GET      /api/v1/dashboard/products    Product analytics
  GET      /api/v1/dashboard/inventory   Inventory insights

------------------------------------------------------------------------

# Revenue Analytics

Metrics:

-   Daily revenue
-   Weekly revenue
-   Monthly revenue
-   Average order value
-   Revenue growth

Supports custom date ranges.

------------------------------------------------------------------------

# Order Analytics

Metrics:

-   Orders by status
-   Completed orders
-   Cancelled orders
-   Refund rate
-   Conversion rate (future)

------------------------------------------------------------------------

# Customer Analytics

Metrics:

-   New customers
-   Returning customers
-   Top customers
-   Customer lifetime value (future)

------------------------------------------------------------------------

# Product Analytics

Metrics:

-   Best sellers
-   Slow-moving products
-   Most viewed products (future)
-   Category performance

------------------------------------------------------------------------

# Inventory Dashboard

Metrics:

-   Low stock
-   Out of stock
-   Inventory value
-   Recent adjustments

------------------------------------------------------------------------

# Activity Feed

Endpoint:

GET /api/v1/dashboard/activity

Shows:

-   Recent orders
-   Refunds
-   Product updates
-   Inventory changes
-   Store configuration changes

------------------------------------------------------------------------

# Filters

Supported:

-   Date range
-   Category
-   Product
-   Status
-   Customer group

------------------------------------------------------------------------

# Export APIs

  Method   Endpoint
  -------- --------------------------------
  GET      /api/v1/dashboard/export/csv
  GET      /api/v1/dashboard/export/excel

Exports respect applied filters.

------------------------------------------------------------------------

# Performance

-   Aggregate queries
-   Cached dashboard widgets
-   AsNoTracking projections
-   Background aggregation for expensive reports

------------------------------------------------------------------------

# Validation

-   Date range required
-   Maximum export size
-   Authorized tenant only

------------------------------------------------------------------------

# Testing

Verify:

-   KPI accuracy
-   Revenue calculations
-   Date filtering
-   Export generation
-   Tenant isolation
-   Cache invalidation

------------------------------------------------------------------------

# Next Document

**32-Super-Admin-APIs.md**

Topics:

-   Tenant management
-   Platform settings
-   Theme moderation
-   Subscription administration
-   Global analytics
