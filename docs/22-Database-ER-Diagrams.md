# Kromic Store Backend Implementation Guide

# Phase 02 -- 22 Database ER Diagrams

**Version:** 1.0\
**Status:** Approved Foundation

------------------------------------------------------------------------

# Purpose

This document provides a high-level reference of the database
relationships, aggregate boundaries, tenant ownership model, and
dependency order.

> **Note:** The diagrams are conceptual and intended to guide
> implementation.

------------------------------------------------------------------------

# Domain Overview

``` mermaid
flowchart LR
    Tenant --> Identity
    Tenant --> Catalog
    Tenant --> Theme
    Tenant --> Customer
    Customer --> Cart
    Cart --> Checkout
    Checkout --> Order
    Order --> Payment
    Order --> Shipment
    Order --> Notification
```

------------------------------------------------------------------------

# Identity

``` mermaid
erDiagram
    User ||--o{ UserRole : has
    Role ||--o{ UserRole : assigned
    User ||--o{ RefreshToken : owns
    User ||--o{ NotificationPreference : configures
```

------------------------------------------------------------------------

# Tenant

``` mermaid
erDiagram
    Tenant ||--|| TenantBranding : has
    Tenant ||--|| TenantSettings : has
    Tenant ||--o{ TenantDomain : owns
    Tenant ||--o{ ThemeAssignment : uses
```

------------------------------------------------------------------------

# Catalog

``` mermaid
erDiagram
    Category ||--o{ Product : contains
    Product ||--o{ ProductVariant : has
    Product ||--o{ ProductImage : has
    Product ||--o{ Inventory : tracks
```

------------------------------------------------------------------------

# Customers & Checkout

``` mermaid
erDiagram
    Customer ||--o{ CustomerAddress : owns
    Customer ||--|| Wishlist : has
    Wishlist ||--o{ WishlistItem : contains
    Customer ||--|| Cart : owns
    Cart ||--o{ CartItem : contains
    Customer ||--o{ CheckoutSession : starts
```

------------------------------------------------------------------------

# Orders

``` mermaid
erDiagram
    Order ||--o{ OrderItem : contains
    Order ||--o{ OrderStatusHistory : tracks
    Order ||--o{ OrderNote : stores
    Order ||--o{ Shipment : ships
    Order ||--o{ Payment : paid_by
    Payment ||--o{ Refund : creates
```

------------------------------------------------------------------------

# Notifications

``` mermaid
erDiagram
    OutboxEvent ||--o{ NotificationLog : produces
    NotificationTemplate ||--o{ NotificationLog : renders
    WebhookSubscription ||--o{ OutboxEvent : receives
```

------------------------------------------------------------------------

# Aggregate Boundaries

  Aggregate      Root
  -------------- -----------------
  Tenant         Tenant
  Identity       User
  Catalog        Product
  Customer       Customer
  Cart           Cart
  Checkout       CheckoutSession
  Order          Order
  Notification   OutboxEvent

------------------------------------------------------------------------

# Tenant Ownership Matrix

  Entity         Tenant Owned
  -------------- -------------------------
  User           Yes
  Customer       Yes
  Product        Yes
  Category       Yes
  Theme          Public/System or Tenant
  Order          Yes
  Payment        Yes
  Notification   Yes

------------------------------------------------------------------------

# Dependency Order

``` text
Tenant
 ↓
Identity
 ↓
Catalog
 ↓
Customer
 ↓
Cart
 ↓
Checkout
 ↓
Order
 ↓
Payment
 ↓
Notification
```

------------------------------------------------------------------------

# Migration Order

1.  Tenant
2.  Identity
3.  Themes
4.  Catalog
5.  Customers
6.  Cart
7.  Checkout
8.  Orders
9.  Payments
10. Notifications

------------------------------------------------------------------------

# CQRS Ownership

-   Catalog → Product commands/queries
-   Customer → Customer commands/queries
-   Cart → Cart commands/queries
-   Checkout → Checkout commands/queries
-   Order → Order commands/queries
-   Notification → Outbox commands/queries

------------------------------------------------------------------------

# Implementation Notes

-   Every tenant-owned table includes `TenantId`.
-   Global query filters enforce tenant isolation and soft delete.
-   Financial records remain immutable after completion.
-   Background workers handle asynchronous workflows.

------------------------------------------------------------------------

# Phase 02 Complete

Phase 02 establishes the complete database foundation for Kromic Store,
including:

-   Database philosophy
-   Entity model
-   Multi-tenancy
-   Authentication
-   Themes
-   Catalog
-   Customers
-   Checkout
-   Orders
-   Notifications
-   Performance
-   EF Core configuration
-   Migrations
-   ER diagrams

------------------------------------------------------------------------

# Next Phase

**Phase 03 -- API & CQRS Design**

Topics include:

-   Endpoint catalog
-   Commands & Queries
-   Request/Response DTOs
-   Validation
-   Authorization
-   Error handling
-   Versioning
-   OpenAPI standards
