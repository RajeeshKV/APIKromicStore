# Kromic Store Backend Implementation Guide

# Phase 02 -- 17 Orders and Payments

**Version:** 1.0\
**Status:** Approved Foundation

------------------------------------------------------------------------

# Purpose

This document defines the order management and payment model for Kromic
Store.

Objectives:

-   Reliable order lifecycle
-   Payment traceability
-   Refund support
-   Shipment tracking
-   Financial consistency

------------------------------------------------------------------------

# Entity Overview

``` text
Order
 ├── OrderItem
 ├── OrderStatusHistory
 ├── OrderNote
 ├── Payment
 ├── Refund
 └── Shipment
```

------------------------------------------------------------------------

# Order

Represents a confirmed purchase.

Columns:

-   Id
-   TenantId
-   OrderNumber
-   CustomerId
-   BillingAddressSnapshot
-   ShippingAddressSnapshot
-   Currency
-   SubTotal
-   DiscountAmount
-   ShippingAmount
-   TaxAmount
-   GrandTotal
-   Status
-   PaymentStatus
-   CreatedOnUtc

Order numbers are unique per tenant.

Indexes:

-   UX_Order_Tenant_OrderNumber
-   IX_Order_Status
-   IX_Order_Customer

------------------------------------------------------------------------

# OrderItem

Stores purchased products.

Columns:

-   OrderId
-   ProductId
-   ProductVariantId (nullable)
-   ProductName
-   SKU
-   Quantity
-   UnitPrice
-   DiscountAmount
-   TaxAmount
-   LineTotal

Product details are stored as snapshots to preserve historical accuracy.

------------------------------------------------------------------------

# Order Status Lifecycle

``` text
Placed
   ↓
Confirmed
   ↓
Processing
   ↓
Shipped
   ↓
Delivered

Cancelled
Refunded
```

Rules:

-   Status transitions are validated.
-   Delivered orders cannot return to Processing.
-   Cancelled orders cannot be shipped.

------------------------------------------------------------------------

# OrderStatusHistory

Tracks every status change.

Columns:

-   OrderId
-   PreviousStatus
-   CurrentStatus
-   ChangedBy
-   ChangedOnUtc
-   Remarks

Used for audit and customer timelines.

------------------------------------------------------------------------

# OrderNote

Internal or customer-visible notes.

Columns:

-   OrderId
-   Note
-   IsVisibleToCustomer
-   CreatedBy
-   CreatedOnUtc

------------------------------------------------------------------------

# Payment

Represents payment attempts.

Columns:

-   OrderId
-   Provider
-   ProviderPaymentId
-   Amount
-   Currency
-   Status
-   PaidOnUtc
-   FailureReason

Supports:

-   Razorpay
-   Future providers

Multiple payment attempts are supported.

------------------------------------------------------------------------

# Refund

Tracks refunds.

Columns:

-   PaymentId
-   Amount
-   RefundReference
-   Status
-   Reason
-   RefundedOnUtc

Supports:

-   Full refund
-   Partial refund

------------------------------------------------------------------------

# Shipment

Stores dispatch information.

Columns:

-   OrderId
-   CourierName
-   TrackingNumber
-   TrackingUrl
-   DispatchedOnUtc
-   DeliveredOnUtc

Multiple shipments may be supported in future.

------------------------------------------------------------------------

# Inventory Rules

-   Reserve inventory during checkout.
-   Deduct inventory after successful order creation.
-   Restore inventory when cancelled before dispatch.
-   Do not automatically restore after delivery.

------------------------------------------------------------------------

# Email Notifications

Trigger emails for:

-   Order placed
-   Payment successful
-   Payment failed
-   Order confirmed
-   Shipment dispatched
-   Delivered
-   Refund processed

Uses Outbox pattern.

------------------------------------------------------------------------

# Business Rules

-   Orders are immutable financial records.
-   Address snapshots are never modified.
-   Product snapshots preserve pricing history.
-   Soft delete does not apply to completed financial records.

------------------------------------------------------------------------

# Recommended Indexes

-   (TenantId, OrderNumber)
-   (TenantId, CustomerId)
-   (TenantId, Status)
-   (PaymentId)
-   (OrderId, ChangedOnUtc)

------------------------------------------------------------------------

# Reporting

Common reports:

-   Daily sales
-   Revenue
-   Refund summary
-   Top customers
-   Top products
-   Pending shipments

------------------------------------------------------------------------

# Testing

Verify:

-   Status transitions
-   Partial refunds
-   Payment retries
-   Shipment updates
-   Inventory restoration
-   Snapshot consistency

------------------------------------------------------------------------

# Future Enhancements

-   Split shipments
-   Multi-currency
-   Gift orders
-   Subscription billing
-   Store credits
-   Invoices

------------------------------------------------------------------------

# Next Document

**18-Outbox-And-Notifications.md**

Topics:

-   Outbox events
-   Email queue
-   Webhooks
-   Notification preferences
-   Background workers
-   Retry policies
