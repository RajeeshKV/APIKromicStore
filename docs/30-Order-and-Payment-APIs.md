# Kromic Store Backend Implementation Guide

# Phase 03 -- 30 Order and Payment APIs

**Version:** 1.0\
**Status:** Approved Foundation

------------------------------------------------------------------------

# Purpose

Define the APIs responsible for order management, payment processing,
fulfillment, refunds, and customer order history.

------------------------------------------------------------------------

# Authorization

  Role           Access
  -------------- ------------------------------
  Customer       Own orders only
  TenantAdmin    Full
  StoreManager   Manage orders (configurable)

------------------------------------------------------------------------

# Order Lifecycle

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

------------------------------------------------------------------------

# Order APIs

  Method   Endpoint                       Description
  -------- ------------------------------ ------------------
  GET      /api/v1/orders                 List orders
  GET      /api/v1/orders/{id}            Order details
  POST     /api/v1/orders/{id}/cancel     Cancel order
  GET      /api/v1/orders/{id}/timeline   Order timeline
  GET      /api/v1/orders/{id}/invoice    Download invoice

Rules:

-   Customers may access only their own orders.
-   Admins can filter by status, customer, and date.

------------------------------------------------------------------------

# Admin Order Management

  Method   Endpoint
  -------- ------------------------------
  PUT      /api/v1/orders/{id}/status
  POST     /api/v1/orders/{id}/notes
  POST     /api/v1/orders/{id}/shipment

Supported transitions:

-   Confirm
-   Process
-   Ship
-   Deliver
-   Cancel

Invalid transitions return HTTP 422.

------------------------------------------------------------------------

# Payment APIs

  Method   Endpoint
  -------- ------------------------------------
  POST     /api/v1/payments/verify
  POST     /api/v1/payments/webhooks/razorpay
  GET      /api/v1/payments/{orderId}

Business Rules:

-   Verify provider signature.
-   Prevent duplicate processing.
-   Record all payment attempts.

------------------------------------------------------------------------

# Refund APIs

  Method   Endpoint
  -------- ----------------------
  POST     /api/v1/refunds
  GET      /api/v1/refunds/{id}

Supports:

-   Full refund
-   Partial refund

Refunds require completed payment.

------------------------------------------------------------------------

# Shipment APIs

  Method   Endpoint
  -------- ------------------------------
  GET      /api/v1/orders/{id}/shipment
  PUT      /api/v1/orders/{id}/shipment

Fields:

-   Courier
-   Tracking Number
-   Tracking URL
-   Dispatch Date
-   Delivery Date

------------------------------------------------------------------------

# Invoice

Invoice endpoint returns a PDF.

Invoice includes:

-   Store details
-   Customer details
-   Tax summary
-   Line items
-   Totals
-   Payment reference

------------------------------------------------------------------------

# Notifications

Trigger events:

-   Order placed
-   Payment verified
-   Order confirmed
-   Shipment dispatched
-   Delivered
-   Cancelled
-   Refunded

Delivered asynchronously using the Outbox pattern.

------------------------------------------------------------------------

# Validation

-   Valid status transition
-   Refund amount \<= paid amount
-   Payment signature verified
-   Shipment requires tracking number

------------------------------------------------------------------------

# Error Scenarios

  Scenario                             Status
  --------------------------- ---------------
  Order not found                         404
  Invalid transition                      422
  Refund exceeds payment                  400
  Invalid payment signature               401
  Duplicate webhook             200 (ignored)

------------------------------------------------------------------------

# Testing

Verify:

-   Order retrieval
-   Status updates
-   Razorpay webhook verification
-   Refund processing
-   Shipment updates
-   Invoice generation
-   Authorization
-   Duplicate webhook handling

------------------------------------------------------------------------

# Next Document

**31-Dashboard-APIs.md**

Topics:

-   Dashboard KPIs
-   Sales analytics
-   Revenue reports
-   Inventory insights
-   Customer metrics
-   Recent activity
